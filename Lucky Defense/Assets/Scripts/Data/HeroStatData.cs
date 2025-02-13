public class StatValueData
{
    public float AtkDamageValue { get; private set; }
    public float AtkSpeedValue { get; private set; }
    public StatValueData(float atkDamageValue, float atkSpeedValue)
    {
        AtkDamageValue = atkDamageValue;
        AtkSpeedValue = atkSpeedValue;
    }
}

public class StatRateData
{
    public float AtkDamageRate { get; private set; }
    public float AtkSpeedRate { get; private set; }

    public StatRateData(float atkDamageRate, float atkSpeedRate)
    {
        AtkDamageRate = atkDamageRate;
        AtkSpeedRate = atkSpeedRate;
    }
}