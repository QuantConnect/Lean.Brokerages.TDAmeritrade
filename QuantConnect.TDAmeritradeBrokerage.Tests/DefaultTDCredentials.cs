using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDAmeritradeApi.Client;

namespace QuantConnect.TDAmeritradeDownloader.Tests
{
    internal class DefaultTDCredentials : ICredentials
    {
        /// <summary>
        /// TD User name
        /// </summary>
        /// <returns></returns>
        public string GetUserName()
        {
            string username = "";

            return username;
        }

        /// <summary>
        /// TD password
        /// </summary>
        /// <returns></returns>
        public string GetPassword()
        {
            //Add breakpoint for live edit
            string password = "";

            return password;
        }

        /// <summary>
        /// TD multi-factor auth code
        /// </summary>
        /// <returns></returns>
        public string GetSmsCode()
        {
            //Add breakpoint for live edit
            string smsCode = "";

            return smsCode;
        }
    }
}
