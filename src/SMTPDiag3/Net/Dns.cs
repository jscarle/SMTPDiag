using System.Collections.Immutable;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace SMTPDiag3.Net;

/// <summary>Defines methods used to query DNS.</summary>
public static partial class Dns
{
    private const short DnsTypeText = 0x0010;
    private const int DnsQueryStandard = 0x00000000;
    private const int DnsSuccess = 0;
    private const int DnsErrorRcodeNameError = 9003;
    private const int DnsInfoNoRecords = 9501;

    /// <summary>Gets the fully qualified domain name (FQDN) of the current host.</summary>
    /// <returns>The FQDN of the current host.</returns>
    public static string GetFqdn()
    {
        var domainName = IPGlobalProperties.GetIPGlobalProperties()
            .DomainName;
        var hostName = System.Net.Dns.GetHostName();
        
        var fqdn = !hostName.Contains(domainName) ? $"{hostName}.{domainName}" : hostName;
        if (fqdn.EndsWith('.'))
            fqdn = fqdn[..^1];

        return fqdn;
    }

    /// <summary>Gets the IP addresses associated with the specified hostname.</summary>
    /// <param name="hostname">The hostname to query for IP addresses.</param>
    /// <returns>A comma-separated string of IP addresses associated with the specified hostname.</returns>
    public static string GetIpAddresses(string hostname)
    {
        var entry = System.Net.Dns.GetHostEntry(hostname);

        var ipAddresses = entry.AddressList.Select(x => x.ToString());

        return string.Join(", ", ipAddresses);
    }
    
    /// <summary>Gets the SPF (Sender Policy Framework) record for the specified domain name.</summary>
    /// <param name="domainName">The domain name to query for the SPF record.</param>
    /// <returns>The SPF record for the specified domain name if found; otherwise, <c>null</c>.</returns>
    public static string? GetSpfRecord(string domainName)
    {
        var txtRecords = GetTxtRecords(domainName);
        return txtRecords.FirstOrDefault(dnsTxtRecord => dnsTxtRecord.StartsWith("v=spf", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> GetTxtRecords(string domain)
    {
        var results = new List<string>();

        var queryResults = IntPtr.Zero;
        try
        {
            var dnsStatus = DnsQuery(domain, DnsTypeText, DnsQueryStandard, IntPtr.Zero, ref queryResults, IntPtr.Zero);

            if (dnsStatus is DnsErrorRcodeNameError or DnsInfoNoRecords)
                return ImmutableArray<string>.Empty;

            if (dnsStatus != DnsSuccess)
                throw new Win32Exception(dnsStatus);

            DnsRecordTxt dnsRecord;
            for (var nextPtr = queryResults; nextPtr != IntPtr.Zero; nextPtr = dnsRecord.pNext)
            {
                dnsRecord = Marshal.PtrToStructure<DnsRecordTxt>(nextPtr);
                if (dnsRecord.wType != DnsTypeText)
                    continue;

                var strArrayPtr = nextPtr + Marshal.OffsetOf<DnsRecordTxt>(nameof(DnsRecordTxt.pStringArray));
                var txtRecord = GetStringArray(strArrayPtr, dnsRecord.dwStringCount);
                results.Add(txtRecord);
            }
        }
        finally
        {
            if (queryResults != IntPtr.Zero)
                DnsRecordListFree(queryResults, (int)DnsFreeType.DnsFreeRecordList);
        }

        return results;
    }

    private static string GetStringArray(IntPtr ptr, int count)
    {
        var builder = new StringBuilder();
        
        var nextPtr = ptr;
        for (var index = 0; index < count; ++index)
        {
            var strPtr = Marshal.PtrToStructure<IntPtr>(nextPtr);
            var str = Marshal.PtrToStringUni(strPtr);
            builder.Append(str);
            
            nextPtr += IntPtr.Size;
        }

        return builder.ToString();
    }

    [LibraryImport("Dnsapi.dll", EntryPoint = "DnsQuery_W", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int DnsQuery(string pszName, short wType, int options, IntPtr pExtra, ref IntPtr ppQueryResults, IntPtr pReserved);

    [LibraryImport("Dnsapi.dll")]
    private static partial void DnsRecordListFree(IntPtr pRecordList, int freeType);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DnsRecordTxt
    {
        public IntPtr pNext;
        public string pName;
        public short wType;
        public short wDataLength;
        public int flags;
        public int dwTtl;
        public int dwReserved;
        public int dwStringCount;
        public string pStringArray;
    }

    // ReSharper disable UnusedMember.Local
    private enum DnsFreeType
    {
        DnsFreeFlat = 0,
        DnsFreeRecordList = 1,
        DnsFreeParsedMessageFields = 2,
    }
}
