// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;
using System.IO;

// still have problems with spaces

namespace Microsoft.Ddue.Tools {

	public class TaskGrabberComponent : BuildComponent {

		private XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());

		private XPathExpression valueQuery = null;
		private XPathExpression keyQuery = null;

		private string xpathFromConfig = string.Empty;

		private Dictionary<string, List<string>> bKeywordMap = new Dictionary<string, List<string>>();

		private Dictionary<string, string> index = new Dictionary<string, string>();

		// what to copy
		private List<CopySet> copySets = new List<CopySet>();

        public TaskGrabberComponent(BuildAssembler assembler, XPathNavigator configuration)
			: base(assembler, configuration)
		{
			XPathNavigator keywordsNode = configuration.SelectSingleNode("keywords");
			if (keywordsNode == null)
				return;

			string filespec = keywordsNode.GetAttribute("files", String.Empty);
            filespec = Environment.ExpandEnvironmentVariables(filespec);
            string keywordXPath = keywordsNode.GetAttribute("keyword", String.Empty);
			string topicXPath = keywordsNode.GetAttribute("topic", String.Empty);
			if (String.IsNullOrEmpty(keywordXPath) || String.IsNullOrEmpty(topicXPath) || String.IsNullOrEmpty(filespec))
				return;

			xpathFromConfig = keywordXPath;

			string[] keywordFiles = null;
			if (File.Exists(filespec))
			{
				// we're loading a single file
				AddBKeywords(filespec, topicXPath, keywordXPath);
			}
			else
			{
				// must be loading a set of files
				if (Directory.Exists(filespec))
				{
					// if they specified a directory, transform all the files in the directory
					keywordFiles = Directory.GetFiles(filespec);
				}
				else
				{
					// it's not a file or a directory, maybe it's a path with wildcards
					string directoryPath = Path.GetDirectoryName(filespec);
					string filePath = Path.GetFileName(filespec);
					keywordFiles = Directory.GetFiles(directoryPath, filePath);
				}
				foreach (string file in keywordFiles)
				{
					// load targets from each file
					AddBKeywords(file, topicXPath, keywordXPath);
				}
			}

			// set up the context
			// put in a default entry for ddue
			nsManager.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/6");
			// look for context nodes, which could override the default
			XPathNodeIterator contextNodes = configuration.Select("context");
			foreach (XPathNavigator contextNode in contextNodes)
			{
				string prefix = contextNode.GetAttribute("prefix", String.Empty);
				string uri = contextNode.GetAttribute("name", String.Empty);
				nsManager.AddNamespace(prefix, uri);
			}
			

			// set up the index of source files
			XPathNodeIterator sourceNodes = configuration.Select("source");
			foreach (XPathNavigator sourceNode in sourceNodes)
			{
				string valueXPath = sourceNode.GetAttribute("value", String.Empty);
				string keyXPath = sourceNode.GetAttribute("key", String.Empty);
				if (String.IsNullOrEmpty(valueXPath) || String.IsNullOrEmpty(keyXPath))
				{
					WriteMessage(MessageLevel.Error, "@key and @value must be set in the 'source' node.");
					return;
				}

				keyQuery = XPathExpression.Compile(keyXPath);
				valueQuery = XPathExpression.Compile(valueXPath);

				// search the data directories for entries
				XPathNodeIterator dataNodes = sourceNode.Select("data");
				foreach (XPathNavigator dataNode in dataNodes)
				{
					string dataFiles = dataNode.GetAttribute("files", String.Empty);
                    dataFiles = Environment.ExpandEnvironmentVariables(dataFiles);
                    if ((dataFiles == null) || (dataFiles.Length == 0)) throw new ConfigurationErrorsException("When instantiating a CopyFromDirectory component, you must specify a directory path using the files attribute.");
					WriteMessage(MessageLevel.Info, String.Format("Searching for files that match '{0}'.", dataFiles));
					int fileCount = ParseDocuments(dataFiles);
					WriteMessage(MessageLevel.Info, String.Format("Found {0} files.", fileCount));
				}
				WriteMessage(MessageLevel.Info, String.Format("Indexed {0} elements.", index.Count));

			}

			// get the copy commands
			XPathNodeIterator copyNodes = configuration.Select("copy");
			foreach (XPathNavigator copyNode in copyNodes)
			{
				string sourceXPath = copyNode.GetAttribute("source", String.Empty);
				string targetXPath = copyNode.GetAttribute("target", String.Empty);
				if (String.IsNullOrEmpty(sourceXPath) || String.IsNullOrEmpty(targetXPath))
				{
					WriteMessage(MessageLevel.Error, "@source and @target must be set in the 'copy' node.");
					return;
				}

				copySets.Add(new CopySet(sourceXPath, targetXPath, nsManager));
			}
			
		}

		private string currentKey = string.Empty;

		public override void Apply (XmlDocument document, string key) {
			currentKey = key;

			XPathNavigator targetDoc = document.CreateNavigator();
			foreach (CopySet copySet in copySets)
			{
				XPathExpression targetExpression = copySet.GetTargetExpression(targetDoc, key);

				// get the target nodes in the document
				XPathNavigator targetNode = targetDoc.SelectSingleNode(targetExpression);
				while (targetNode != null)
				{
					string targetId = targetNode.Value;
					int pound = (string.IsNullOrEmpty(targetId)) ? -1 : targetId.IndexOf("#");
					string bkeyword = (pound == -1) ? "" : targetId.Substring(pound + 1);
					if (bkeyword == "")
					{
						base.WriteMessage(key, MessageLevel.Warn, "Invalid id '{0}' in topic '{1}'.", targetId, currentKey);
						// delete this target and get the next target node
						targetNode.DeleteSelf();
						targetNode = targetDoc.SelectSingleNode(targetExpression);
						continue;
					}

					List<string> idList;
					if (!bKeywordMap.TryGetValue(bkeyword, out idList))
					{
						base.WriteMessage(key, MessageLevel.Warn, "B-keyword not found '{0}' in topic '{1}'.", targetId, currentKey);
						// delete this target and get the next target node
						targetNode.DeleteSelf();
						targetNode = targetDoc.SelectSingleNode(targetExpression);
						continue;
					}
					if (idList.Count > 1)
						Console.Write("");

					// create a 'tasks' node to replace the target
					XPathNavigator tasksNode = document.CreateElement("tasks").CreateNavigator();
					tasksNode.CreateAttribute(string.Empty, "bkeyword", string.Empty, bkeyword);
					foreach (string topicId in idList)
					{
						//create a task node for this source topic
						XPathNavigator taskNode = document.CreateElement("task").CreateNavigator();
						taskNode.CreateAttribute(string.Empty, "topicId", string.Empty, topicId);

						// get the source document for the topic id
                        string filepath;
                        if (!index.TryGetValue(topicId, out filepath))
                        {
                            base.WriteMessage(key, MessageLevel.Warn, "No file found for topicId '{0}' for " +
                                "B-keyword '{1}'. Source topic: '{2}'.", topicId, bkeyword, currentKey);
                            continue;
                        }

						XPathNavigator sourceDoc = new XPathDocument(filepath).CreateNavigator();
						XPathNavigator sourceNode = sourceDoc.SelectSingleNode(valueQuery);

						if (sourceNode == null) continue;
						XPathNodeIterator sources = sourceNode.Select(copySet.SourceExpression);

						// append the source nodes to the target node
						if (sources.Count > 0)
						{
							foreach (XPathNavigator source in sources)
								taskNode.AppendChild(source);
						}
						tasksNode.AppendChild(taskNode);
					}
					targetNode.ReplaceSelf(tasksNode);
					// get the next target node
					targetNode = targetDoc.SelectSingleNode(targetExpression);
				}
			}
		}

		public int ParseDocuments(string wildcardPath)
		{
			string directoryPart = Path.GetDirectoryName(wildcardPath);
			if (String.IsNullOrEmpty(directoryPart)) directoryPart = Environment.CurrentDirectory;
			directoryPart = Path.GetFullPath(directoryPart);
			string filePart = Path.GetFileName(wildcardPath);
			string[] files = Directory.GetFiles(directoryPart, filePart);
			foreach (string file in files)
				ParseDocument(file);
			return (files.Length);
		}

		private void ParseDocument(string file)
		{
			try
			{
				XPathDocument document = new XPathDocument(file);

				// set context for the xpath expression
				valueQuery.SetContext(nsManager);
				keyQuery.SetContext(nsManager);

				XPathNodeIterator valueNodes = document.CreateNavigator().Select(valueQuery);
				foreach (XPathNavigator valueNode in valueNodes)
				{
					XPathNavigator keyNode = valueNode.SelectSingleNode(keyQuery);
					if (keyNode == null) continue;
					string key = keyNode.Value;

					// log multiple occurences of a single id
					if (index.ContainsKey(key))
					{
						WriteMessage(MessageLevel.Warn, String.Format("Entries for the key '{0}' occur in both '{1}' and '{2}'. The first entry will be used.", key, index[key], file));
					}
					else
					{
						index[key] = file;
					}

				}
			}
			catch (Exception e)
			{
				WriteMessage(MessageLevel.Error, e.Message);
			}
		}

		private XPathDocument GetDocument(string identifier)
		{
			string file = index[identifier];
			XPathDocument document = new XPathDocument(file);
			return (document);
		}

		private void AddBKeywords(string file, string topicXPath, string keywordXPath)
		{
			XPathDocument document = new XPathDocument(file);
			XPathNodeIterator targetNodes = document.CreateNavigator().Select(topicXPath);
			foreach (XPathNavigator targetNode in targetNodes)
			{
				string topicId = targetNode.GetAttribute("id", string.Empty);
				if (string.IsNullOrEmpty(topicId))
					continue;
				foreach (XPathNavigator keywordNode in targetNode.Select(keywordXPath))
				{
					string keyword = keywordNode.Value;
					if (string.IsNullOrEmpty(keyword))
						continue;
					AddValueToListDictionary(bKeywordMap, keyword, topicId);
				}
			}
		}

		public static void AddValueToListDictionary<K, V>(Dictionary<K, List<V>> dict, K key, V value)
		{
			List<V> list;
			try
			{
				if (!dict.TryGetValue(key, out list))
				{
					list = new List<V>();
					dict.Add(key, list);
				}
				list.Add(value);
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception adding to dictionary {0}", key);
				Console.WriteLine(e);
				throw;
			}
		}

	}

	internal class CopySet
	{

		public CopySet(string sourceXPath, string targetXPath, XmlNamespaceManager nsMgr)
			: this(null, sourceXPath, targetXPath, nsMgr)
		{
		}

		public CopySet(IndexedFileCache cache, string sourceXPath, string targXPath, XmlNamespaceManager nsMgr)
		{
			this.fileCache = cache;
			this.namespaceMgr = nsMgr;
			source = XPathExpression.Compile(sourceXPath);
			source.SetContext(nsMgr);
			if (targXPath.Contains("{0}"))
			{
				targetXPath = targXPath;
			}
			else
			{
				target = XPathExpression.Compile(targXPath);
				target.SetContext(nsMgr);
			}
		}

		private IndexedFileCache fileCache;
		public IndexedFileCache FileCache
		{
			get
			{
				return (fileCache);
			}
		}

		private XPathExpression source;
		public XPathExpression SourceExpression
		{
			get
			{
				return (source);
			}
		}

		private XmlNamespaceManager namespaceMgr; 

		private string targetXPath = string.Empty;
		private XPathExpression target = null;
		public XPathExpression GetTargetExpression(XPathNavigator targetDoc, string key)
		{
			if (target == null)
			{
				XPathExpression keyedTargetExpression = targetDoc.Compile(string.Format(targetXPath, key));
				keyedTargetExpression.SetContext(namespaceMgr);
				return keyedTargetExpression;
			}
			
			return target;
		}


	}


}