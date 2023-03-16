# SPY

SPY is a learning game focused on CT. The game principle is to select from a list of actions, those that will allow a robot to get out of a maze. These actions are represented as blocks and the player has to build a sequence of actions that will be sent to the robot for execution.

To get out of the maze, the player has to program the robot to avoid obstacles. These obstacles have been built for educational purposes:
 - Sentinels end the level if they spot the robot (if the robot passes through their detection area). Players can select every sentinel to see the sequence of actions assigned to it. Players must know how to read programs, to understand it and to anticipate the sentinels' movements in order to create a proper sequence to reach the objective with the robot.
 - Doors can be activated with terminals. This feature was designed to engage a step-by-step resolution process. Players have to understand that to reach the exit, it is necessary to solve the level in sub-steps (Objective 1: Activate the terminal to open the door. Objective 2: Reach the exit). This also allows players to manipulate the states of an object (open or closed).
 - Program several robots with a unique program. Players need to find a generic solution that enables two robots to reach their exit in different mazes.
 
In sum players have to observe and model the simulation (abstraction), decompose their strategy in sub-steps (decomposition), determine the best solution (evaluation), plan the actions to perform (algorithmic thinking) and reuse and adapt previous solutions on new problems (generalization).

You can play the game at: [https://spy.lip6.fr/](https://spy.lip6.fr/)

SPY is developed with Unity and the [FYFY](https://github.com/Mocahteam/FYFY) library.
