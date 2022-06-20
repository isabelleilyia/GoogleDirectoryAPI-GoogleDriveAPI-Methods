using System;
namespace ValidationMethods
{
    class Validation
    {
        public static bool validateGradeNumber(int gradYear)
        {
            if (gradYear >= DateTime.Now.Year && gradYear <= DateTime.Now.Year + 12)
            {
                return true;
            }

            return false;
        }
    }
}