using UnityEngine;

namespace RPG.Combat
{
    public class AttackReport
    {
        public GameObject instigator;
        public int damageDealt = 0;
        public AttackResult result;

        public AttackReport()
        {
            result = AttackResult.None;
        }

        public AttackReport(AttackResult result, GameObject instigator)
        {
            this.result = result;
            this.instigator = instigator;
        }
    }

    public enum AttackResult
    {
        None,
        Hit,
        Miss,
        CriticalHit,
        TargetDown
    }
}
