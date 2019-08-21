namespace RPG.Combat
{
    public class AttackReport
    {
        public int damageDealt = 0;
        public AttackResult result;

        public AttackReport()
        {
            result = AttackResult.None;
        }

        public AttackReport(AttackResult result)
        {
            this.result = result;
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
