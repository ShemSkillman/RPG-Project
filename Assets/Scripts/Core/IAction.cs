using System;
using UnityEngine;

namespace RPG.Core
{
    // Sheduler can cancel any action in progress
    public interface IAction
    {
        void Cancel();
    }
}

