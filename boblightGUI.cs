﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Xml;

namespace boblight_net
{
    public partial class boblightGUI : Form
    {
        private boblightClient client;
        public boblightGUI()
        {
            InitializeComponent();
            XmlReader reader = XmlReader.Create("bobClient.xml");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Server")
                {
                    textIP.Text = reader.GetAttribute("IP");
                    textPort.Text = reader.GetAttribute("Port");
                    textPriority.Text = reader.GetAttribute("Priority");
                }
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (client == null || client.isConnected() == false)
            {
                client = new boblightClient(textIP.Text, Int32.Parse(textPort.Text), Int32.Parse(textPriority.Text));
                if (client.isConnected())
                {
                    panelProperties.Enabled = true;
                    tabsControl.Enabled = true;
                    buttonMisc.Enabled = true;
                    textIP.Enabled = false;
                    textPort.Enabled = false;
                    textPriority.Enabled = false;
                    lblStatus.Text = "Connected";
                    buttonConnect.Text = "Disconnect";
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create("bobClient.xml", settings);
                    writer.WriteStartDocument();
                    writer.WriteComment("Last used boblightConfiguration");
                    writer.WriteStartElement("Server");
                    writer.WriteAttributeString("IP", textIP.Text);
                    writer.WriteAttributeString("Port", textPort.Text);
                    writer.WriteAttributeString("Priority", textPriority.Text);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();

                    writer.Flush();
                    writer.Close();
                    foreach (light light in client.getLights())
                    {
                        listLights.Items.Add(light.getName());
                    }
                }
            }
            else
            {
                client.disconnect();
                listLights.Items.Clear();
                panelProperties.Enabled = false;
                tabsControl.Enabled = false;
                buttonMisc.Enabled = false;
                textIP.Enabled = true;
                textPort.Enabled = true;
                textPriority.Enabled = true;
                lblStatus.Text = "Disconnected";
                buttonConnect.Text = "Connect";
            }
        }

        private light[] getSelectedLights()
        {
            if (tabsControl.SelectedIndex == tabLights.TabIndex)
            {
                light[] lights = new light[listLights.SelectedIndices.Count];
                int i = 0;
                foreach (light light in client.getLights())
                {
                    if (listLights.SelectedItems.Contains(light.getName()))
                    {
                        lights[i] = light;
                        i++;
                    }
                }
                return lights;
            }
            else
            {
                ICollection<light> lightsCollection = new List<light>();
                foreach (lightGroup lGroup in listGroups.SelectedItems)
                {
                    foreach (light light in lGroup.lights)
                    {
                        lightsCollection.Add(light);
                    }
                }
                return lightsCollection.ToArray();
            }
            
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            client.setColor(getSelectedLights(), textHex.Text, 1.0F);
            client.syncLights();
        }

        private void buttonSpeed_Click(object sender, EventArgs e)
        {
            client.setSpeed(getSelectedLights(), float.Parse(textSpeed.Text));
            client.syncLights();
        }

        private void buttonUse_Click(object sender, EventArgs e)
        {
            client.setUse(getSelectedLights(), checkUse.Checked);
            client.syncLights();
        }

        private void tabsControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabsControl.SelectedIndex == tabLights.TabIndex)
            {
                buttonMisc.Text = "Group Lights";
            }
            else if (tabsControl.SelectedIndex == tabGroups.TabIndex)
            {
                buttonMisc.Text = "Delete Group";
            }
        }

        private void buttonMisc_Click(object sender, EventArgs e)
        {
            if (tabsControl.SelectedIndex == tabLights.TabIndex)
            {
                string groupName = Interaction.InputBox("Please enter a name for this light group:", "New Light Group");
                if (!String.IsNullOrWhiteSpace(groupName) && listLights.SelectedItems.Count != 0)
                {
                    listGroups.Items.Add(new lightGroup(getSelectedLights(), groupName));
                    tabsControl.SelectedIndex = tabGroups.TabIndex;
                }
            }
            else if(tabsControl.SelectedIndex == tabGroups.TabIndex)
            {
                while (listGroups.SelectedItems.Count != 0)
                {
                    listGroups.Items.Remove(listGroups.SelectedItems[0]);
                }
            }
        }
    }
}
