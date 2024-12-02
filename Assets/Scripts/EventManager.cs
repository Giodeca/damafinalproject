using System;
using UnityEngine;

public static class EventManager
{
    public static Action ClearBoard;
    public static Action ClearList;
    public static Action<GameObject> OnPiecePromoted;
    public static Action TurnChange;
    public static Action Attack;

}
