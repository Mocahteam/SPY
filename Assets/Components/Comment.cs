using UnityEngine;

// Ce script permet d'ajouter un commentaire dans l'inspecteur Unity pour expliquer le rôle d'un GameObject ou d'un composant. Il est utile pour documenter le projet et faciliter la compréhension du code par d'autres développeurs.
public class Comment : MonoBehaviour
{
    [TextArea(3, 10)]
    public string comment;
}
