using System;

namespace Scryfall.Api
{
    public partial class Error : Exception
    {
        public Error() : base()
        {
        }

        public Error(string message) : base(message)
        {
        }

        public Error(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}