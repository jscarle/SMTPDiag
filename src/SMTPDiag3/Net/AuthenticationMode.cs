namespace SMTPDiag3.Net;

/// <summary>Specifies the authentication modes used for authentication.</summary>
public enum AuthenticationMode
{
    /// <summary>No authentication.</summary>
    None = 0,

    /// <summary>Plain text authentication.</summary>
    Plain = 1,

    /// <summary>LOGIN authentication.</summary>
    Login = 2,

    /// <summary>CRAM-MD5 authentication.</summary>
    CramMd5 = 3,

    /// <summary>Force plain text authentication.</summary>
    ForcePlain = 4,

    /// <summary>Force LOGIN authentication.</summary>
    ForceLogin = 5,

    /// <summary>Force CRAM-MD5 authentication.</summary>
    ForceCramMd5 = 6,
}
