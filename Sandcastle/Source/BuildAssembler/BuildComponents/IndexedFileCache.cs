// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
	// the storage system

	internal class IndexedFileCache
	{

		public IndexedFileCache(string keyXPath, string valueXPath, int cacheSize)
		{
			valueExpression = XPathExpression.Compile(valueXPath);

			keyExpression = XPathExpression.Compile(keyXPath);

			_cacheSize = cacheSize;

			cache = new Dictionary<string, IndexedFile>(_cacheSize);

			lruLinkedList = new LinkedList<string>();
		}

		// search pattern for value nodes to be mapped
		private XPathExpression valueExpression;

		// search pattern for the key identifier (relative to the value node)
		private XPathExpression keyExpression;

		// an index mapping topic IDs to files
		private Dictionary<string, string> idToFileMap = new Dictionary<string, string>();

		public int ParseDocuments(string wildcardPath)
		{
			string directoryPart = Path.GetDirectoryName(wildcardPath);
			if (String.IsNullOrEmpty(directoryPart)) directoryPart = Environment.CurrentDirectory;
			directoryPart = Path.GetFullPath(directoryPart);
			string filePart = Path.GetFileName(wildcardPath);
			string[] files = Directory.GetFiles(directoryPart, filePart);
			// WriteMessage(MessageLevel.Info, String.Format("Found {0} files.", files.Length) );
			foreach (string file in files)
			{
				ParseDocument(file);
			}
			return (files.Length);
		}

		private void ParseDocument(string file)
		{
			try
			{
				XPathDocument document = new XPathDocument(file);
				XPathNodeIterator valueNodes = document.CreateNavigator().Select(valueExpression);
				foreach (XPathNavigator valueNode in valueNodes)
				{
					XPathNavigator keyNode = valueNode.SelectSingleNode(keyExpression);
					if (keyNode == null) continue;
					string key = keyNode.Value;

					// log multiple occurences of a single id
					if (idToFileMap.ContainsKey(key))
					{
						// WriteMessage(MessageLevel.Warn, String.Format("Entries for the key '{0}' occur in both '{1}' and '{2}'. The first entry will be used.", key, idToFileMap[key], file));
					}
					else
					{
						idToFileMap[key] = file;
					}
				}
			}
			catch (XmlException)
			{
				// WriteMessage(MessageLevel.Error, e.Message);
			}
		}

		// a simple document caching mechanism

		private int _cacheSize = 10;

		private LinkedList<String> lruLinkedList;

		private Dictionary<string, IndexedFile> cache;


		private IndexedFile GetCachedDocument(string identifier)
		{
			string file;
			if (idToFileMap.TryGetValue(identifier, out file))
			{
				IndexedFile document;
				if (cache.TryGetValue(file, out document))
				{
					// move the file from its current position to the head of the lru linked list
					lruLinkedList.Remove(document.ListNode);
					lruLinkedList.AddFirst(document.ListNode);
				}
				else
				{
					// not in the cache, so load and index a new source file
					document = new IndexedFile(file, valueExpression, keyExpression);
					if (cache.Count >= _cacheSize)
					{
						// the cache is full
						// the last node in the linked list has the path of the next file to remove from the cache
						if (lruLinkedList.Last != null)
						{
							cache.Remove(lruLinkedList.Last.Value);
							lruLinkedList.RemoveLast();
						}
					}
					// add the new file to the cache and to the head of the lru linked list
					cache.Add(file, document);
					document.ListNode = lruLinkedList.AddFirst(file);
				}
				return (document);
			}
			else
			{
				return (null);
			}
		}

		public XPathNavigator GetContent(string identifier)
		{

			// get the document containing the identifier
			IndexedFile document = GetCachedDocument(identifier);
			if (document == null) return (null);

			// select the comment part of the document
			return document.GetContent(identifier);
		}

		public int Count
		{
			get
			{
				return (idToFileMap.Count);
			}
		}

	}

	internal class IndexedFile
	{
		Dictionary<string, XPathNavigator> valueIndex = new Dictionary<string, XPathNavigator>();

		public IndexedFile(string filepath, XPathExpression valueExpression, XPathExpression keyExpression)
		{
			XPathDocument xpDoc = new XPathDocument(filepath);
			XPathNodeIterator valueNodes = xpDoc.CreateNavigator().Select(valueExpression);
			foreach (XPathNavigator valueNode in valueNodes)
			{
				XPathNavigator keyNode = valueNode.SelectSingleNode(keyExpression);
				if (keyNode == null)
					continue;
				string key = keyNode.Value;
				if (!valueIndex.ContainsKey(key))
					valueIndex.Add(key, valueNode);
			}
		}

		public XPathNavigator GetContent(string key)
		{
			return valueIndex[key];
		}

		private LinkedListNode<string> listNode;
		public LinkedListNode<string> ListNode
		{
			get
			{
				return (listNode);
			}
			set
			{
				listNode = value;
			}
		}

	}

}
