namespace SwiftMES.IIL.Client
{
    public  class DbResult<T>
    {
        public bool IsSucceed;
        public string message;

        public DbResult(bool IsSucceed)
        {
            this.IsSucceed = IsSucceed;
        }

        public DbResult(bool v, string message)
        {
            this.IsSucceed = v;
            this.message = message;
        }
    }
}