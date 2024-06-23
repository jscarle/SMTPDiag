namespace SMTPDiag3.Net;

/// <summary>Represents errors that occur during SMTP operations.</summary>
public class SmtpException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="SmtpException"/> class.</summary>
    public SmtpException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SmtpException"/> class with a specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public SmtpException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpException"/> class with a specified error message and a reference to the inner exception that is the
    /// cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public SmtpException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
