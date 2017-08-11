namespace UniCorn.Http
{
    internal class PumpLimits
    {
        internal int MaxOutstandingAccepts { get; private set; }

        internal int MaxOutstandingRequests { get; private set; }

        internal PumpLimits(int maxAccepts, int maxRequests)
        {
            MaxOutstandingAccepts = maxAccepts;

            MaxOutstandingRequests = maxRequests;
        }
    }
}