using DisruptorUnity3d;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinCan;
using TinCan.LRSResponses;
using UnityEngine;

namespace DIG.GBLXAPI.Internal
{
    public class LrsRemoteQueue : MonoBehaviour
    {
		public const string GAMEOBJECT_NAME = "GBLXAPI";

		// ************************************************************************
		// Monobehaviour singleton
		// ************************************************************************
		private static LrsRemoteQueue instance = null;
		public static LrsRemoteQueue Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (new GameObject(GAMEOBJECT_NAME)).AddComponent<LrsRemoteQueue>();
				}

				return instance;
			}
		}

		public bool useDefaultCallback = false;

		private List<RemoteLRSAsync> _lrsEndpoints; // WebGL/Desktop/Mobile coroutine implementation of RemoteLRS.cs

		private RingBuffer<QueuedStatement> _statementQueue;

        private int batchTreshold = 1000;
		private int totalqueuedStatement = 0;

		// ------------------------------------------------------------------------
		// Set singleton so it persists across scene loads
		// ------------------------------------------------------------------------
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		public void Init(List<GBLConfig> configs, int queueDepth = 2000)
		{
            _lrsEndpoints = new List<RemoteLRSAsync>();
            foreach (GBLConfig config in configs)
                _lrsEndpoints.Add(new RemoteLRSAsync(config.lrsConfig));
			_statementQueue = new RingBuffer<QueuedStatement>(queueDepth);
		}

		private void Update()
        {
            if (_statementQueue == null || _statementQueue.Count < batchTreshold)
                return;
            flushQueuedStatements(false);
        }

        private void OnDestroy()
        {
			// flush statements
            flushQueuedStatements(false);
			Debug.Log("Total statements sent:" + totalqueuedStatement);
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
				flushQueuedStatements(false);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
				flushQueuedStatements(false);
		}


		public void flushQueuedStatements(bool waitComplete)
        {
			if (_statementQueue != null)
			{
				// Dequeue statements if exists in queue
				List<QueuedStatement> batchStatements = new List<QueuedStatement>();
				while (_statementQueue.Count > 0)
				{
					if (_statementQueue.TryDequeue(out QueuedStatement queuedStatement))
					{
						// Debug statement
						if (GBLXAPI.debugMode)
						{
							Debug.Log(queuedStatement.statement.ToJSON(true));
						}
						batchStatements.Add(queuedStatement);
					}
				}
				if (batchStatements.Count > 0)
				{
					// Send to each endPoint
					foreach (RemoteLRSAsync endPoint in _lrsEndpoints)
					{
						if (waitComplete)
						{
							StartCoroutine(SendStatementCoroutine(endPoint, batchStatements));
						}
						else
						{
							Task _ = SendStatements(endPoint, batchStatements);
						}

					}
				}
			}
        }

        public void EnqueueStatement(Statement statement, Action<string, bool, string> sendCallback = null)
		{
			// Make sure all required fields are set
			bool valid = true;
			string invalidReason = "";
			if (statement.actor == null) { valid = false; invalidReason += "ERROR: Agent is null\n"; }
			if (statement.verb == null) { valid = false; invalidReason += "ERROR: Verb is null\n"; }
			if (statement.target == null) { valid = false; invalidReason += "ERROR: Object is null\n"; }

			// Use default callback if none was given
			if (sendCallback == null && useDefaultCallback)
			{
				sendCallback = StatementDefaultCallback;
			}

			if (valid)
			{
				// Check if space in the ringbuffer queue, if not discard or will hard lock unity
				if (_statementQueue.Capacity - _statementQueue.Count > 0)
				{
					totalqueuedStatement++;
					_statementQueue.Enqueue(new QueuedStatement(statement, sendCallback));
				}
				else
				{
					Debug.LogWarning("QueueStatement: Queue is full. Discarding Statement");
				}
			}
			else
			{
				sendCallback?.Invoke("", false, invalidReason);
			}
		}

        private async Task<StatementLRSResponse> SendStatements(RemoteLRSAsync endPoint, List<QueuedStatement> queuedStatements)
        {
            List<Statement> statements = new List<Statement>();
            foreach (QueuedStatement qs in queuedStatements)
                statements.Add(qs.statement);
			return await endPoint.PostStatements(statements);
        }

        // ------------------------------------------------------------------------
        // This coroutine spawns a thread to send the statement to the LRS
        // ------------------------------------------------------------------------
        private IEnumerator SendStatementCoroutine(RemoteLRSAsync endPoint, List<QueuedStatement> queuedStatements)
		{
			var sendTask = SendStatements(endPoint, queuedStatements);
			// Wait answer
			yield return new WaitUntil(() => sendTask.IsCompleted);

			bool success = sendTask.Status == TaskStatus.RanToCompletion && sendTask.Result.success;
			var errMessage = sendTask.Status == TaskStatus.RanToCompletion ? sendTask.Result.errMsg : "Operation not completed";

			if (sendTask.Status == TaskStatus.RanToCompletion && sendTask.Result.success) {
				Debug.Log("Statements successfully sent");
			}
			else
			{
				Debug.LogWarning("Statements failed with error: " + errMessage);
				Debug.LogWarning("Try again...");
				yield return new WaitForSeconds(0.5f);
				StartCoroutine(SendStatementCoroutine(endPoint, queuedStatements));
			}

			// Client callback with result
			foreach (QueuedStatement qs in queuedStatements) {
                qs.callback?.Invoke(endPoint.xApiEndpoint, success, errMessage);
			}
		}

		private void StatementDefaultCallback(string endpoint, bool result, string resultText)
		{
			if (result) { Debug.Log("GBLXAPI: "+ endpoint + " SUCCESS: " + resultText); }
			else { Debug.Log("GBLXAPI: "+endpoint+" ERROR: " + resultText); }
		}

		public struct QueuedStatement
		{
			public Statement statement;
			public Action<string, bool, string> callback;

			public QueuedStatement(Statement statement, Action<string, bool, string> callback)
			{
				this.statement = statement;
				this.callback = callback;
			}
		}
	}
}
