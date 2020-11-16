using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Docusign_Demo.Models;
using Newtonsoft.Json;

namespace Docusign_Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SendDocumentforSign()
        {
            return View();
        }

        MyCredential credential = new MyCredential();
        private string INTEGRATOR_KEY = "b24c81a9-89c7-4c74-a762-c62d2066ecb1";
        
        [HttpPost]
        public ActionResult SendDocumentforSign(Docusign_Demo.Models.Recipient recipient, HttpPostedFileBase UploadDocument)
        {
            Recipient recipientModel = new Recipient();
            string directorypath = Server.MapPath("~/App_Data/" + "Files/");
            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);
            }
            byte[] data;
            using (Stream inputStream = UploadDocument.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                data = memoryStream.ToArray();
            }
            var serverpath = directorypath + recipient.Name.Trim() + ".pdf";
            System.IO.File.WriteAllBytes(serverpath, data);
            docusign(serverpath, recipient.Name, recipient.Email);
            return View();
        }
        public string loginApi(string usr, string pwd)
        {
            var basePath = "https://demo.docusign.net/restapi"; // Base API path
            // we set the api client in global config when we configured the client  
            ApiClient apiClient = new ApiClient(basePath);
            string authHeader = "{\"Username\":\"" + usr + "\", \"Password\":\"" + pwd + "\", \"IntegratorKey\":\"" + INTEGRATOR_KEY + "\"}";
            Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

            // we will retrieve this from the login() results
            string accountId = null;

            // the authentication api uses the apiClient (and X-DocuSign-Authentication header) that are set in Configuration object
            AuthenticationApi authApi = new AuthenticationApi();
            LoginInformation loginInfo = authApi.Login();

            // find the default account for this user
            foreach (DocuSign.eSign.Model.LoginAccount loginAcct in loginInfo.LoginAccounts)
            {
                if (loginAcct.IsDefault == "true")
                {
                    accountId = loginAcct.AccountId;
                    break;
                }
            }
            if (accountId == null)
            { // if no default found set to first account
                accountId = loginInfo.LoginAccounts[0].AccountId;
            }
            return accountId;
        }
        public void docusign(string path, string recipientName, string recipientEmail)
        {
           

            //Verify Account Details
            string accountId = loginApi(credential.UserName, credential.Password);

            // Read a file from disk to use as a document.
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);

            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Please Sign for Tone Tone";

            // Add a document to the envelope
            Document doc = new Document();
            doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
            doc.Name = Path.GetFileName(path);
            doc.DocumentId = "1";

            envDef.Documents = new List<Document>();
            envDef.Documents.Add(doc);

            // Add a recipient to sign the documeent
            DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
            signer.Email = recipientEmail;
            signer.Name = recipientName;
            signer.RecipientId = "1";

            envDef.Recipients = new DocuSign.eSign.Model.Recipients();
            envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
            envDef.Recipients.Signers.Add(signer);

            //set envelope status to "sent" to immediately send the signature request
            envDef.Status = "sent";

            // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
            EnvelopesApi envelopesApi = new EnvelopesApi();
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);

            // print the JSON response
            var result = JsonConvert.SerializeObject(envelopeSummary);
        }
    }
    public class MyCredential
    {
        public string UserName
        {
            get;
            set;
        } = "xxxx@gmail.com";
        public string Password
        {
            get;
            set;
        } = "xxxx";
    }

}