using System;

[Serializable]
public class ProductionTime
{
    public float prodTime = 20;
    public float currentTime = 0;
    public int modifier = 1;
    public ProductionTime(int _prodTime, float _currentTime, int _modifier)
    {
        this.prodTime = _prodTime;
        this.currentTime = _currentTime;
        this.modifier = _modifier;
    }
    public ProductionTime()
    {

    }
}