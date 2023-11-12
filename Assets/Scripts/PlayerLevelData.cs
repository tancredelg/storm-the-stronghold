[System.Serializable]
public class PlayerLevelData
{
    public int[] LevelSystemData;
    public int SkillPoints;
    public int NextMilestone;

    public override string ToString()
    {
        return $"[PlayerLevelData]\nSkillPoints: {SkillPoints}\nNextMileStone: {NextMilestone}";
    }
}
