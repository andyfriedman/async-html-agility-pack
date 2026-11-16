// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.


using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using System.Diagnostics.CodeAnalysis;

namespace HtmlAgilityPack
{
	/// <summary>
	/// Represents an HTML navigator on an HTML document seen as a data store.
	/// </summary>
	public class HtmlNodeNavigator : XPathNavigator
	{
		#region Fields

		private int _attindex;
		private HtmlNode _currentnode;
		private readonly HtmlDocument _doc;
		private readonly HtmlNameTable _nametable;


		#endregion

		#region Constructors

		internal HtmlNodeNavigator()
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			Reset();
		}

		internal HtmlNodeNavigator(HtmlDocument doc, HtmlNode currentNode)
		{
			if (currentNode == null)
			{
				throw new ArgumentNullException("currentNode");
			}

			if (currentNode.OwnerDocument != doc)
			{
				throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
			}

			if (doc == null)
			{
				// keep in message, currentNode.OwnerDocument also null.
				throw new Exception("Oops! The HtmlDocument cannot be null.");
			}


			_doc = doc;
			_nametable = new HtmlNameTable();
			Reset();
			_currentnode = currentNode;
		}

		private HtmlNodeNavigator(HtmlNodeNavigator nav)
		{
			if (nav == null)
			{
				throw new ArgumentNullException("nav");
			}
			_doc = nav._doc;
			_currentnode = nav._currentnode;
			_attindex = nav._attindex;
			_nametable = nav._nametable; // REVIEW: should we do this?
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		public HtmlNodeNavigator(Stream stream)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(stream, _doc.OptionDefaultStreamEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
		public HtmlNodeNavigator(Stream stream, bool detectEncodingFromByteOrderMarks)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, bufferSize: -1, leaveOpen: true);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public HtmlNodeNavigator(Stream stream, Encoding encoding)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
		public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize: -1, leaveOpen: true);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
		/// <param name="buffersize">The minimum buffer size.</param>
		public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize, leaveOpen: true);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a TextReader.
		/// </summary>
		/// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
		public HtmlNodeNavigator(TextReader reader)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			_doc.LoadInternal(reader);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		public HtmlNodeNavigator(string path)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(path, _doc.OptionDefaultStreamEncoding);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		public HtmlNodeNavigator(string path, bool detectEncodingFromByteOrderMarks)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(path, detectEncodingFromByteOrderMarks);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public HtmlNodeNavigator(string path, Encoding encoding)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(path, encoding);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks);
			_doc.LoadInternal(sr);
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
		/// <param name="buffersize">The minimum buffer size.</param>
		public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
		{
			_doc = new HtmlDocument();
			_nametable = new HtmlNameTable();
			using var sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize);
			_doc.LoadInternal(sr);
			Reset();
		}


		/// <summary>
		/// Asynchronously creates a new <see cref="HtmlNodeNavigator"/> and loads an HTML document from a stream.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		public static async Task<HtmlNodeNavigator> CreateAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			var doc = new HtmlDocument();
			await doc.LoadAsync(stream, cancellationToken).ConfigureAwait(false);
			return new HtmlNodeNavigator(doc, doc.DocumentNode);
		}

		/// <summary>
		/// Asynchronously creates a new <see cref="HtmlNodeNavigator"/> and loads an HTML document from a stream with the specified encoding.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		public static async Task<HtmlNodeNavigator> CreateAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
		{
			var doc = new HtmlDocument();
			await doc.LoadAsync(stream, encoding, cancellationToken).ConfigureAwait(false);
			return new HtmlNodeNavigator(doc, doc.DocumentNode);
		}

		/// <summary>
		/// Asynchronously creates a new <see cref="HtmlNodeNavigator"/> and loads an HTML document from a <see cref="TextReader"/>.
		/// </summary>
		/// <param name="reader">The reader used to feed the HTML data into the document.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		public static async Task<HtmlNodeNavigator> CreateAsync(TextReader reader, CancellationToken cancellationToken = default)
		{
			var doc = new HtmlDocument();
			await doc.LoadAsync(reader, cancellationToken).ConfigureAwait(false);
			return new HtmlNodeNavigator(doc, doc.DocumentNode);
		}

		/// <summary>
		/// Asynchronously creates a new <see cref="HtmlNodeNavigator"/> and loads an HTML document from a file path.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		public static async Task<HtmlNodeNavigator> CreateAsync(string path, CancellationToken cancellationToken = default)
		{
			var doc = new HtmlDocument();
			await doc.LoadAsync(path, cancellationToken).ConfigureAwait(false);
			return new HtmlNodeNavigator(doc, doc.DocumentNode);
		}

		/// <summary>
		/// Asynchronously creates a new <see cref="HtmlNodeNavigator"/> and loads an HTML document from a file path with the specified encoding.
		/// </summary>
		/// <param name="path">The complete file path to be read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		public static async Task<HtmlNodeNavigator> CreateAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
		{
			var doc = new HtmlDocument();
			await doc.LoadAsync(path, encoding, cancellationToken).ConfigureAwait(false);
			return new HtmlNodeNavigator(doc, doc.DocumentNode);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the base URI for the current node.
		/// Always returns string.Empty in the case of HtmlNavigator implementation.
		/// </summary>
		public override string BaseURI
		{
			get
			{
				return _nametable.GetOrAdd(string.Empty);
			}
		}

		/// <summary>
		/// Gets the current HTML document.
		/// </summary>
		public HtmlDocument CurrentDocument
		{
			get { return _doc; }
		}

		/// <summary>
		/// Gets the current HTML node.
		/// </summary>
		public HtmlNode CurrentNode
		{
			get { return _currentnode; }
		}

		/// <summary>
		/// Gets a value indicating whether the current node has child nodes.
		/// </summary>
		public override bool HasAttributes
		{
			get
			{
				return (_currentnode.Attributes.Count > 0);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current node has child nodes.
		/// </summary>
		public override bool HasChildren
		{
			get
			{
				return (_currentnode.ChildNodes.Count > 0);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current node is an empty element.
		/// </summary>
		public override bool IsEmptyElement
		{
			get
			{
				// REVIEW: is this ok?
				return !HasChildren;
			}
		}

		/// <summary>
		/// Gets the name of the current HTML node without the namespace prefix.
		/// </summary>
		public override string LocalName
		{
			get
			{
				if (_attindex != -1)
				{
					return _nametable.GetOrAdd(_currentnode.Attributes[_attindex].Name);
				}

				return _nametable.GetOrAdd(_currentnode.Name);
			}
		}

		/// <summary>
		/// Gets the qualified name of the current node.
		/// </summary>
		public override string Name
		{
			get
			{
				return _nametable.GetOrAdd(_currentnode.Name);
			}
		}

		/// <summary>
		/// Gets the namespace URI (as defined in the W3C Namespace Specification) of the current node.
		/// Always returns string.Empty in the case of HtmlNavigator implementation.
		/// </summary>
		public override string NamespaceURI
		{
			get
			{
				return _nametable.GetOrAdd(string.Empty);
			}
		}

		/// <summary>
		/// Gets the <see cref="XmlNameTable"/> associated with this implementation.
		/// </summary>
		public override XmlNameTable NameTable
		{
			get
			{
				return _nametable;
			}
		}

		/// <summary>
		/// Gets the type of the current node.
		/// </summary>
		public override XPathNodeType NodeType
		{
			get
			{
				switch (_currentnode.NodeType)
				{
					case HtmlNodeType.Comment:
						return XPathNodeType.Comment;

					case HtmlNodeType.Document:
						return XPathNodeType.Root;

					case HtmlNodeType.Text:
						return XPathNodeType.Text;

					case HtmlNodeType.Element:
						{
							if (_attindex != -1)
							{
								return XPathNodeType.Attribute;
							}

							return XPathNodeType.Element;
						}

					default:
						throw new NotImplementedException("Internal error: Unhandled HtmlNodeType: " +
														  _currentnode.NodeType);
				}
			}
		}

		/// <summary>
		/// Gets the prefix associated with the current node.
		/// Always returns string.Empty in the case of HtmlNavigator implementation.
		/// </summary>
		public override string Prefix
		{
			get
			{
				return _nametable.GetOrAdd(string.Empty);
			}
        }

        /// <summary>
        /// Gets the text value of the current node.
        /// </summary>
        public override string Value
        {
			get
			{
				switch (_currentnode.NodeType)
				{
					case HtmlNodeType.Comment:
						return ((HtmlCommentNode) _currentnode).Comment;

					case HtmlNodeType.Document:
						return "";

					case HtmlNodeType.Text:
						return ((HtmlTextNode) _currentnode).Text;

					case HtmlNodeType.Element:
						{
							if (_attindex != -1)
							{
								return _currentnode.Attributes[_attindex].Value ?? string.Empty;
							}

							return _currentnode.InnerText;
						}

					default:
						throw new NotImplementedException("Internal error: Unhandled HtmlNodeType: " +
														  _currentnode.NodeType);
				}
			}
		}

		/// <summary>
		/// Gets the xml:lang scope for the current node.
		/// Always returns string.Empty in the case of HtmlNavigator implementation.
		/// </summary>
		public override string XmlLang
		{
			get
			{
				return _nametable.GetOrAdd(string.Empty);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a new HtmlNavigator positioned at the same node as this HtmlNavigator.
		/// </summary>
		/// <returns>A new HtmlNavigator object positioned at the same node as the original HtmlNavigator.</returns>
		public override XPathNavigator Clone()
		{
			return new HtmlNodeNavigator(this);
		}

		/// <summary>
		/// Gets the value of the HTML attribute with the specified LocalName and NamespaceURI.
		/// </summary>
		/// <param name="localName">The local name of the HTML attribute.</param>
		/// <param name="namespaceURI">The namespace URI of the attribute. Unsupported with the HtmlNavigator implementation.</param>
		/// <returns>The value of the specified HTML attribute. String.Empty or null if a matching attribute is not found or if the navigator is not positioned on an element node.</returns>
		public override string GetAttribute(string localName, string namespaceURI)
		{
			HtmlAttribute? att = _currentnode.HasAttributes ? _currentnode.Attributes[localName] : null;
			return att?.Value ?? string.Empty;
		}

		/// <summary>
		/// Returns the value of the namespace node corresponding to the specified local name.
		/// Always returns string.Empty for the HtmlNavigator implementation.
		/// </summary>
		/// <param name="name">The local name of the namespace node.</param>
		/// <returns>Always returns string.Empty for the HtmlNavigator implementation.</returns>
		public override string GetNamespace(string name)
		{
			return string.Empty;
		}

		/// <summary>
		/// Determines whether the current HtmlNavigator is at the same position as the specified HtmlNavigator.
		/// </summary>
		/// <param name="other">The HtmlNavigator that you want to compare against.</param>
		/// <returns>true if the two navigators have the same position, otherwise, false.</returns>
		public override bool IsSamePosition(XPathNavigator other)
		{
            HtmlNodeNavigator? nav = other as HtmlNodeNavigator;

            if (nav == null)
			{
				return false;
			}

			return (nav._currentnode == _currentnode);
		}

		/// <summary>
		/// Moves to the same position as the specified HtmlNavigator.
		/// </summary>
		/// <param name="other">The HtmlNavigator positioned on the node that you want to move to.</param>
		/// <returns>true if successful, otherwise false. If false, the position of the navigator is unchanged.</returns>
		public override bool MoveTo(XPathNavigator other)
		{
            HtmlNodeNavigator? nav = other as HtmlNodeNavigator;

            if (nav == null)
			{
				return false;
			}


			if (nav._doc == _doc)
			{
				_currentnode = nav._currentnode;
				_attindex = nav._attindex;
				return true;
			}

			// we don't know how to handle that
			return false;
		}

		/// <summary>
		/// Moves to the HTML attribute with matching LocalName and NamespaceURI.
		/// </summary>
		/// <param name="localName">The local name of the HTML attribute.</param>
		/// <param name="namespaceURI">The namespace URI of the attribute. Unsupported with the HtmlNavigator implementation.</param>
		/// <returns>true if the HTML attribute is found, otherwise, false. If false, the position of the navigator does not change.</returns>
		public override bool MoveToAttribute(string localName, string namespaceURI)
		{
			int index = _currentnode.Attributes.GetAttributeIndex(localName);
			if (index == -1)
			{
				return false;
			}

			_attindex = index;
			return true;
		}

		/// <summary>
		/// Moves to the first sibling of the current node.
		/// </summary>
		/// <returns>true if the navigator is successful moving to the first sibling node, false if there is no first sibling or if the navigator is currently positioned on an attribute node.</returns>
		public override bool MoveToFirst()
		{
			if (_currentnode.ParentNode == null)
			{
				return false;
			}

			if (_currentnode.ParentNode.FirstChild == null)
			{
				return false;
			}

			_currentnode = _currentnode.ParentNode.FirstChild;
			return true;
		}

		/// <summary>
		/// Moves to the first HTML attribute.
		/// </summary>
		/// <returns>true if the navigator is successful moving to the first HTML attribute, otherwise, false.</returns>
		public override bool MoveToFirstAttribute()
		{
			if (!HasAttributes)
			{
				return false;
			}

			_attindex = 0;
			return true;
		}

		/// <summary>
		/// Moves to the first child of the current node.
		/// </summary>
		/// <returns>true if there is a first child node, otherwise false.</returns>
		public override bool MoveToFirstChild()
		{
			if (!_currentnode.HasChildNodes)
			{
				return false;
			}

			_currentnode = _currentnode.ChildNodes[0];
			return true;
		}

		/// <summary>
		/// Moves the XPathNavigator to the first namespace node of the current element.
		/// Always returns false for the HtmlNavigator implementation.
		/// </summary>
		/// <param name="scope">An XPathNamespaceScope value describing the namespace scope.</param>
		/// <returns>Always returns false for the HtmlNavigator implementation.</returns>
		public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
		{
			return false;
		}

		/// <summary>
		/// Moves to the node that has an attribute of type ID whose value matches the specified string.
		/// </summary>
		/// <param name="id">A string representing the ID value of the node to which you want to move. This argument does not need to be atomized.</param>
		/// <returns>true if the move was successful, otherwise false. If false, the position of the navigator is unchanged.</returns>
		public override bool MoveToId(string id)
		{
            HtmlNode? node = _doc.GetElementbyId(id);

            if (node == null)
			{
				return false;
			}

			_currentnode = node;
			return true;
		}

		/// <summary>
		/// Moves the XPathNavigator to the namespace node with the specified local name. 
		/// Always returns false for the HtmlNavigator implementation.
		/// </summary>
		/// <param name="name">The local name of the namespace node.</param>
		/// <returns>Always returns false for the HtmlNavigator implementation.</returns>
		public override bool MoveToNamespace(string name)
		{
			return false;
		}

		/// <summary>
		/// Moves to the next sibling of the current node.
		/// </summary>
		/// <returns>true if the navigator is successful moving to the next sibling node, false if there are no more siblings or if the navigator is currently positioned on an attribute node. If false, the position of the navigator is unchanged.</returns>
		public override bool MoveToNext()
		{
			if (_currentnode.NextSibling == null)
			{
				return false;
			}

			_currentnode = _currentnode.NextSibling;
			return true;
		}

		/// <summary>
		/// Moves to the next HTML attribute.
		/// </summary>
		/// <returns></returns>
		public override bool MoveToNextAttribute()
		{
			if (_attindex >= (_currentnode.Attributes.Count - 1))
			{
				return false;
			}

			_attindex++;
			return true;
		}

		/// <summary>
		/// Moves the XPathNavigator to the next namespace node.
		/// Always returns falsefor the HtmlNavigator implementation.
		/// </summary>
		/// <param name="scope">An XPathNamespaceScope value describing the namespace scope.</param>
		/// <returns>Always returns false for the HtmlNavigator implementation.</returns>
		public override bool MoveToNextNamespace(XPathNamespaceScope scope)
		{
			return false;
		}

		/// <summary>
		/// Moves to the parent of the current node.
		/// </summary>
		/// <returns>true if there is a parent node, otherwise false.</returns>
		public override bool MoveToParent()
		{
			if (_currentnode.ParentNode == null)
			{
				return false;
			}

			_currentnode = _currentnode.ParentNode;
			return true;
		}

		/// <summary>
		/// Moves to the previous sibling of the current node.
		/// </summary>
		/// <returns>true if the navigator is successful moving to the previous sibling node, false if there is no previous sibling or if the navigator is currently positioned on an attribute node.</returns>
		public override bool MoveToPrevious()
		{
			if (_currentnode.PreviousSibling == null)
			{
				return false;
			}

			_currentnode = _currentnode.PreviousSibling;
			return true;
		}

		/// <summary>
		/// Moves to the root node to which the current node belongs.
		/// </summary>
		public override void MoveToRoot()
		{
			_currentnode = _doc.DocumentNode;
		}

        #endregion

        #region Internal Methods
        #endregion

        #region Private Methods

        [MemberNotNull(nameof(_currentnode))]
        private void Reset()
		{
			_currentnode = _doc.DocumentNode;
			_attindex = -1;
		}

		#endregion
	}
}
