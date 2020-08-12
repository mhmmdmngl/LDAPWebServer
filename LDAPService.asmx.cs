using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;

namespace LDAPWebServer
{
    //@Author Muhammed Eminoğlu @github account ==> mhmmdmngl


    /// <summary>
    /// Summary description for LDAPService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class LDAPService : System.Web.Services.WebService
    {
        private readonly string _hostName = "yourLDAPCluster.yourdomain.com.tr";
        private readonly int _portNumber = 389;
        private readonly string _username = "";
        private readonly string _password = "";

        
        public readonly string _uid = "YourLdapUserName";
        //usually it is People in default case
        public readonly string _ou = "People";

        public readonly string _o = "yourdomain.com.tr";

        //if your domain name is "yourdomain.com.tr" you need 3 dc, if it is yourdomail.com you don't need dc3 just comment it.
        public readonly string dc1 = "yourdomain";
        public readonly string dc2 = "com";
        //usually you will not need this
        public readonly string dc3 = "tr";




        [WebMethod]
        public bool isValidated(string userName, string password)
        {
            //if your domain yourdomain.com format, just delete ",dc=" + dc3 part;
            string networkCreditString = ",ou=" + _ou + ",o=" + _o + ",dc=" + dc1 + ",dc=" + dc2 + ",dc=" + dc3;
            LdapConnection lc = ConnectLDAP(networkCreditString);
            if (lc != null)
            {
               
                var x = SearchPerson(lc, userName, networkCreditString);

                var answer = retrievePersonData(x, lc, userName, "inetUserStatus");
                //Sometime ldap returns "Active", sometime "active" than i make toLower() string.
                if (answer.ToLower() == "active")
                {

                    var result = validateUserByBind(networkCreditString,lc, userName, password);

                    return result;
                }
            }
            return false;

        }

        public string getUserAttribute(string userName, string attributeName)
        {
            //if your domain yourdomain.com format, just delete ",dc=" + dc3 part;
            string networkCreditString = ",ou=" + _ou + ",o=" + _o + ",dc=" + dc1 + ",dc=" + dc2 + ",dc=" + dc3;
            LdapConnection lc = ConnectLDAP(networkCreditString);
            if (lc != null)
            {

                var x = SearchPerson(lc, userName, networkCreditString);

                var answer = retrievePersonData(x, lc, userName, "inetUserStatus");
                //Sometime ldap returns "Active", sometime "active" than i make toLower() string.
                if (answer.ToLower() == "active")
                {

                    return retrievePersonData(x, lc, userName, attributeName);
                }
                else
                {
                    return "This Account is inActive";
                }
            }
            return "";

        }

        //This function returns 
        public LdapConnection ConnectLDAP(string networkCreditString)
        {
            LdapConnection ldc = null;

            
            networkCreditString = "uid=" + _uid + networkCreditString;
            try
            {
                ldc = new LdapConnection(new LdapDirectoryIdentifier(_hostName, 389));
                ldc.SessionOptions.ProtocolVersion = 3;
                NetworkCredential nc = new NetworkCredential(networkCreditString, _password); //password

                ldc.AuthType = AuthType.Basic;
                ldc.Bind(nc);


            }
            catch
            {

            }

            return ldc;
        }

        public bool validateUserByBind(string networkCreditString, LdapConnection lc, string username, string password)
        {
            bool result = false;
            networkCreditString = "uid=" + username + networkCreditString;
            NetworkCredential nc = new NetworkCredential(networkCreditString,
                password);

            lc.AuthType = AuthType.Basic;
            try
            {
                lc.Bind(nc);
                result = true;
            }
            catch
            {

            }
            return result;

        }

        //This function SearchPerson as given username in base dn
        public SearchResponse SearchPerson(LdapConnection lc, string username, string networkCreditString)
        {
            networkCreditString = "uid=" + username + networkCreditString;
            //You can edit searchFilter as your demand
            string searchFilter = "(objectClass=person)";
            var request1 = new SearchRequest(networkCreditString, searchFilter, System.DirectoryServices.Protocols.SearchScope.Subtree, null);
            var response = (SearchResponse)lc.SendRequest(request1);


            return response;
        }

        //This function retrieve person's data as given ldap attribute name
        public string retrievePersonData(SearchResponse sr, LdapConnection lc, string username, string attributeName)
        {
            var t = sr.Entries[0].Attributes[attributeName][0].ToString();

            return t;
        }


    }
}
