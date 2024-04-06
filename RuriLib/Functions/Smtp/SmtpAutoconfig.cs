﻿using RuriLib.Functions.Networking;
using System.Collections.Generic;
using System.Xml;

namespace RuriLib.Functions.Smtp;

public static class SmtpAutoconfig
{
    public static List<HostEntry> Parse(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var servers =
            doc.DocumentElement.SelectNodes("/clientConfig/emailProvider/outgoingServer[contains(@type,'smtp')]");

        var hosts = new List<HostEntry>();

        foreach (XmlNode server in servers)
        {
            var hostname = server.SelectSingleNode("hostname").FirstChild.Value;
            var port = server.SelectSingleNode("port").FirstChild.Value;

            hosts.Add(new HostEntry(hostname, int.Parse(port)));
        }

        return hosts;
    }
}