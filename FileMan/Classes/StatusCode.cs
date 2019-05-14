using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Classes
{
    public enum StatusCode
    {
        // Generral codes
        Success,
        Error,
        ExceptionThrown,
        CannotCreateDirectory,
        DocumentNotFound,
        DocumentLocked,
        CategoryNotFound
    }

    public class StatusResult
    {
        public bool Success;
        public bool Failure {
            set {
                this.Failure = !Success;
            }
        }
        public StatusCode Status;
        public string Message;

        public StatusResult (bool Success, StatusCode Status, string Message)
        {
            this.Success = Success;
            this.Status = Status;
            this.Message = Message;
        }
    }
}