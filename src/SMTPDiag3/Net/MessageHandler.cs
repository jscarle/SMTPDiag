namespace SMTPDiag3.Net;

/// <summary>Represents the method that will handle a message event.</summary>
/// <param name="elapsedMs">The elapsed time in milliseconds.</param>
/// <param name="message">The message to handle.</param>
public delegate void MessageHandler(long elapsedMs, string message);
