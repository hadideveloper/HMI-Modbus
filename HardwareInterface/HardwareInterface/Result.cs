using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class Result
    {
        public bool Success { set; get; }
        public ErrorCodes? ErrorCode { set; get; }
        public string ErrorMessage { set; get; }

        public static Result Okay
        {
            get
            {
                return new Result()
                {
                    Success = true,
                };
            }
        }

        public static Result UnknownError
        {
            get
            {
                return new Result()
                {
                    Success = false,
                    ErrorCode = ErrorCodes.UnknownError,
                    ErrorMessage = "Unknown error",
                };
            }
        }

        public static Result CanNotOpenPort
        {
            get
            {
                return new Result()
                {
                    Success = false,
                    ErrorCode = ErrorCodes.CanNotOpenPort,
                    ErrorMessage = "Can not open port",
                };
            }
        }

        public static Result PortIsNotOpened
        {
            get
            {
                return new Result()
                {
                    Success = false,
                    ErrorCode = ErrorCodes.PortIsNotOpened,
                    ErrorMessage = "Port is not opened, Call OpenPort first",
                };
            }
        }

        public static Result AlreadyStarted
        {
            get
            {
                return new Result()
                {
                    Success = false,
                    ErrorCode = ErrorCodes.AlreadyStarted,
                    ErrorMessage = "Has already stared, Call Stop first",
                };
            }
        }

        public static Result NotImplemented
        {
            get
            {
                return new Result()
                {
                    Success = false,
                    ErrorCode = ErrorCodes.NotImplemented,
                    ErrorMessage = "This feature not implemented yet",
                };
            }
        }

        public static Result FullBuffer
        {
            get
            {
                return new Result()
                {
                    Success = false,
                    ErrorCode = ErrorCodes.FullBuffer,
                    ErrorMessage = "Buffer is full",
                };
            }
        }
    }
}
