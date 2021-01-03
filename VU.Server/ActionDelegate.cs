namespace VU.Server
{
    public delegate void ActionDelegate();
    public delegate void ActionDelegate<T>(T args);
    public delegate void ActionDelegate<T1, T2>(T1 arg1, T2 arg2);
}
