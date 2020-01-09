using System;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A registry that holds GameObject products by Guid.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Process the given message
        /// </summary>
        /// <param name="message">The message</param>
        void Process(object message);
    }
}
