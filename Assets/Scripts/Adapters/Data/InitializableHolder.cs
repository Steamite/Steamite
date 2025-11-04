public abstract class InitializableHolder<CATEG_T, WRAPPER_T> : DataHolder<CATEG_T, WRAPPER_T> where CATEG_T : DataCategory<WRAPPER_T> where WRAPPER_T : DataObject
{
    /// <summary>
    /// Initialize Modifiers
    /// </summary>
    public abstract void Init();
}

