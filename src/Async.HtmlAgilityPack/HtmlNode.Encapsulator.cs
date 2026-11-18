// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.


using Async.HtmlAgilityPack.Attributes;
using Async.HtmlAgilityPack.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.XPath;

namespace Async.HtmlAgilityPack
{
    public partial class HtmlNode
    {
        /// <summary>
        /// Fill an object and go through it's properties and fill them too.
        /// </summary>
        /// <typeparam name="T">Type of object to want to fill. It should have atleast one property that defined XPath.</typeparam>
        /// <returns>Returns an object of type T including Encapsulated data.</returns>
        /// <exception cref="ArgumentException">Why it's thrown.</exception>
        /// <exception cref="ArgumentNullException">Why it's thrown.</exception>
        /// <exception cref="MissingMethodException"><see cref="MissingMethodException"/></exception>
        /// <exception cref="MissingXPathException"><see cref="MissingXPathException"/></exception>
        /// <exception cref="XPathException">see XPathExeption.</exception>
        /// <exception cref="NodeNotFoundException"><see cref="NodeNotFoundException"/></exception>
        /// <exception cref="NodeAttributeNotFoundException"><see cref="NodeAttributeNotFoundException"/></exception>
        /// <exception cref="FormatException">Why it's thrown.</exception>
        /// <exception cref="Exception">Why it's thrown.</exception>
        /// <exception cref="InvalidNodeReturnTypeException"><see cref="InvalidNodeReturnTypeException"/></exception>
        public T? GetEncapsulatedData<T>()
        {
            return (T?)GetEncapsulatedData(typeof(T), null);
        }


        /// <summary>
        /// Fill an object and go through it's properties and fill them too.
        /// </summary>
        /// <typeparam name="T">Type of object to want to fill. It should have atleast one property that defined XPath.</typeparam>
        /// <param name="htmlDocument">If htmlDocument includes data , leave this parameter null. Else pass your specific htmldocument.</param>
        /// <returns>Returns an object of type T including Encapsulated data.</returns>
        /// <exception cref="ArgumentException">Why it's thrown.</exception>
        /// <exception cref="ArgumentNullException">Why it's thrown.</exception>
        /// <exception cref="MissingMethodException"><see cref="MissingMethodException"/></exception>
        /// <exception cref="MissingXPathException"><see cref="MissingXPathException"/></exception>
        /// <exception cref="XPathException">see XPathExeption.</exception>
        /// <exception cref="NodeNotFoundException"><see cref="NodeNotFoundException"/></exception>
        /// <exception cref="NodeAttributeNotFoundException"><see cref="NodeAttributeNotFoundException"/></exception>
        /// <exception cref="FormatException">Why it's thrown.</exception>
        /// <exception cref="Exception">Why it's thrown.</exception>
        /// <exception cref="InvalidNodeReturnTypeException"><see cref="InvalidNodeReturnTypeException"/></exception>
        public T? GetEncapsulatedData<T>(HtmlDocument htmlDocument)
        {
            return (T?)GetEncapsulatedData(typeof(T), htmlDocument);
        }



        /// <summary>
        /// Fill an object and go through it's properties and fill them too.
        /// </summary>
        /// <param name="targetType">Type of object to want to fill. It should have atleast one property that defined XPath.</param>
        /// <param name="htmlDocument">If htmlDocument includes data , leave this parameter null. Else pass your specific htmldocument.</param>
        /// <returns>Returns an object of type targetType including Encapsulated data.</returns>
        /// <exception cref="ArgumentException">Why it's thrown.</exception>
        /// <exception cref="ArgumentNullException">Why it's thrown.</exception>
        /// <exception cref="MissingMethodException"><see cref="MissingMethodException"/></exception>
        /// <exception cref="MissingXPathException"><see cref="MissingXPathException"/></exception>
        /// <exception cref="XPathException">see XPathExeption.</exception>
        /// <exception cref="NodeNotFoundException"><see cref="NodeNotFoundException"/></exception>
        /// <exception cref="NodeAttributeNotFoundException"><see cref="NodeAttributeNotFoundException"/></exception>
        /// <exception cref="FormatException">Why it's thrown.</exception>
        /// <exception cref="Exception">Why it's thrown.</exception>
        /// <exception cref="InvalidNodeReturnTypeException"><see cref="InvalidNodeReturnTypeException"/></exception>
        public object? GetEncapsulatedData(Type targetType, HtmlDocument? htmlDocument = null)
        {

            #region SettingPrerequisite

            if (targetType == null)
            {
                throw new ArgumentNullException("Parameter targetType is null");
            }

            HtmlDocument source;

            if (htmlDocument == null)
            {
                source = OwnerDocument;
            }
            else
            {
                source = htmlDocument;
            }


            object? targetObject;

            if (targetType.IsInstantiable() == false) // if it can not create instanse of T because of lack of constructor in type T.
            {
                throw new MissingMethodException("Parameterless Constructor excpected for " + targetType.FullName);
            }
            else
            {
                targetObject = Activator.CreateInstance(targetType);
            }

            #endregion SettingPrerequisite

            #region targetObject_Defined_XPath
            if (targetType.IsDefinedAttribute(typeof(HasXPathAttribute)) == true) // Object has xpath attribute (Defined HasXPath)
            {

                // Store list of properties that defined xpath attribute
                PropertyInfo[] validProperties = targetType.GetPropertiesDefinedXPath().ToArray();
                if (validProperties.Length == 0) // if no XPath property exist in type T while T defined HasXpath attribute.
                {
                    throw new MissingXPathException("Type " + targetType.FullName +
                        " defined HasXPath Attribute but it does not have any property with XPath Attribte.");
                }
                else
                {
                    // Fill targetObject variable Properties ( T targetObject )
                    foreach (PropertyInfo propertyInfo in validProperties)
                    {
                        XPathAttribute xPathAttribute = propertyInfo.GetCustomAttribute<XPathAttribute>(inherit: false)!;


                        #region Property_IsNOT_IEnumerable
                        if (propertyInfo.IsIEnumerable() == false) // Property is None-IEnumerable
                        {
                            HtmlNode? htmlNode = null;

                            // try to fill htmlNode based on XPath given
                            try
                            {
                                htmlNode = source.DocumentNode.SelectSingleNode(xPathAttribute.XPath);
                            }
                            catch (XPathException ex) // if it can not select node based on given xpath
                            {
                                throw new XPathException(ex.Message + " That means you have a syntax error in XPath property of this Property : " +
                                    propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                            }
                            catch (Exception ex)
                            {
                                throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " +
                                    propertyInfo.PropertyType.FullName + " " + propertyInfo.Name, ex);
                            }


                            if (htmlNode == null) // If Encapsulator could not find Node.
                            {
                                if (propertyInfo.IsDefined(typeof(SkipNodeNotFoundAttribute), false) == true)
                                {
                                    // set default value.
                                }
                                else
                                {
                                    throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " +
                                    propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                                }

                            }
                            else // if htmlNode is not null (Encapsulator find the Node)
                            {

                                #region Property_Is_HasXPath_UserDefinedClass
                                // Property is None-IEnumerable HasXPath-user-defined class
                                if (propertyInfo.PropertyType.IsDefinedAttribute(typeof(HasXPathAttribute)) == true)
                                {
                                    HtmlDocument innerHtmlDocument = new HtmlDocument();

                                    innerHtmlDocument.LoadHtml(Tools.GetHtmlForEncapsulation(
                                        htmlNode,
                                        xPathAttribute.IsNodeReturnTypeExplicitlySet ? xPathAttribute.NodeReturnType : ReturnType.InnerHtml));

                                    object? o = GetEncapsulatedData(propertyInfo.PropertyType, innerHtmlDocument);

                                    propertyInfo.SetValue(targetObject, o, null);
                                }
                                #endregion Property_Is_HasXPath_UserDefinedClass

                                #region Property_Is_SimpleType
                                // Property is None-IEnumerable value-type or .Net class or user-defined class.
                                // AND does not deifned xpath and shouldn't have property that defined xpath.
                                else
                                {
                                    string? result;

                                    if (xPathAttribute.AttributeName == null) // It target value of HTMLTag
                                    {
                                        result = Tools.GetNodeValueBasedOnXPathReturnType<string>(htmlNode, xPathAttribute);
                                    }
                                    else // It target attribute of HTMLTag
                                    {
                                        ThrowIfNodeReturnTypeIsExplicitlySetWhenAttributeNameIsGiven(xPathAttribute);
                                        result = htmlNode.GetAttributeValue(xPathAttribute.AttributeName, null);
                                    }

                                    if (result == null)
                                    {
                                        throw new NodeAttributeNotFoundException("Can not find " +
                                            xPathAttribute.AttributeName + " Attribute in " + htmlNode.Name +
                                            " related to " + propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                                    }


                                    object resultCastedToTargetPropertyType;

                                    try
                                    {
                                        resultCastedToTargetPropertyType = Convert.ChangeType(result, propertyInfo.PropertyType);
                                    }
                                    catch (FormatException)
                                    {
                                        throw new FormatException("Can not convert Invalid string to " + propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Unhandled Exception : " + ex.Message);
                                    }



                                    propertyInfo.SetValue(targetObject, resultCastedToTargetPropertyType, null);
                                }
                                #endregion Property_Is_SimpleType
                            }

                        }
                        #endregion Property_IsNOT_IEnumerable


                        #region Property_Is_IEnumerable
                        else // Property is IEnumerable<T>
                        {
                            IList<Type>? T_Types = propertyInfo.GetGenericTypes() as IList<Type>; // Get T type

                            if (T_Types == null || T_Types.Count == 0)
                            {
                                throw new ArgumentException(propertyInfo.Name + " should have one generic argument.");
                            }

                            else if (T_Types.Count > 1)
                            {
                                throw new ArgumentException(propertyInfo.Name + " should have one generic argument.");
                            }

                            else if (T_Types.Count == 1) // It is NOT something like Dictionary<Tkey , Tvalue>
                            {
                                HtmlNodeCollection? nodeCollection;

                                // try to fill nodeCollection based on given xpath.
                                try
                                {
                                    nodeCollection = source.DocumentNode.SelectNodes(xPathAttribute.XPath);
                                }
                                catch (XPathException ex)
                                {
                                    throw new XPathException(ex.Message + " That means you have a syntax error in XPath property of this Property : " +
                                        propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                                }
                                catch (Exception ex)
                                {
                                    throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " +
                                        propertyInfo.PropertyType.FullName + " " + propertyInfo.Name, ex);
                                }


                                if (nodeCollection == null || nodeCollection.Count == 0)
                                {
                                    if (propertyInfo.IsDefined(typeof(SkipNodeNotFoundAttribute), false) == true)
                                    {
                                        // set default value.
                                    }
                                    else
                                    {
                                        throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " +
                                        propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                                    }
                                }
                                else
                                {
                                    IList result = T_Types[0].CreateIListOfType();

                                    #region Property_Is_IEnumerable<HasXPath-UserDefinedClass>
                                    if (T_Types[0].IsDefinedAttribute(typeof(HasXPathAttribute)) == true) // T is IEnumerable HasXPath-user-defined class (T type Defined XPath properties)
                                    {
                                        foreach (HtmlNode node in nodeCollection)
                                        {
                                            HtmlDocument innerHtmlDocument = new HtmlDocument();
                                            innerHtmlDocument.LoadHtml(Tools.GetHtmlForEncapsulation(
                                                node,
                                                xPathAttribute.IsNodeReturnTypeExplicitlySet ? xPathAttribute.NodeReturnType : ReturnType.InnerHtml));

                                            object? o = GetEncapsulatedData(T_Types[0], innerHtmlDocument);

                                            result.Add(o);
                                        }
                                    }
                                    #endregion Property_Is_IEnumerable<HasXPath-UserDefinedClass>

                                    #region Property_Is_IEnumerable<SimpleClass>
                                    else // T is value-type or .Net class or user-defined class ( without xpath )
                                    {
                                        if (xPathAttribute.AttributeName == null) // It target value
                                        {
                                            try
                                            {
                                                result = Tools.GetNodesValuesBasedOnXPathReturnType(nodeCollection, xPathAttribute, T_Types[0]);
                                            }
                                            catch (FormatException)
                                            {
                                                throw new FormatException("Can not convert Invalid string in node collection to " + T_Types[0].FullName + " " + propertyInfo.Name);
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception("Unhandled Exception : " + ex.Message);
                                            }
                                        }
                                        else // It target attribute
                                        {
                                            ThrowIfNodeReturnTypeIsExplicitlySetWhenAttributeNameIsGiven(xPathAttribute);

                                            foreach (HtmlNode node in nodeCollection)
                                            {
                                                string? nodeAttributeValue = node.GetAttributeValue(xPathAttribute.AttributeName, null);

                                                if (nodeAttributeValue == null)
                                                {
                                                    throw new NodeAttributeNotFoundException("Can not find " + xPathAttribute.AttributeName + " Attribute in " + node.Name + " related to " +
                                                    propertyInfo.PropertyType.FullName + " " + propertyInfo.Name);
                                                }

                                                object resultCastedToTargetPropertyType;


                                                try
                                                {
                                                    resultCastedToTargetPropertyType = Convert.ChangeType(nodeAttributeValue, T_Types[0]);
                                                }
                                                catch (FormatException) // if it can not cast result(string) to type of property.
                                                {
                                                    throw new FormatException("Can not convert Invalid string to " + T_Types[0].FullName + " " + propertyInfo.Name);
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception("Unhandled Exception : " + ex.Message);
                                                }


                                                result.Add(resultCastedToTargetPropertyType);
                                            }
                                        }
                                    }
                                    #endregion Property_Is_IEnumerable<SimpleClass>

                                    if (result == null || result.Count == 0)
                                    {
                                        throw new Exception("Cannot fill " + propertyInfo.PropertyType.FullName + " " + propertyInfo.Name + " because it is null.");
                                    }

                                    propertyInfo.SetValue(targetObject, result, null);
                                }

                            }
                        }
                        #endregion Property_Is_IEnumerable
                    }

                    return targetObject;
                }
            }
            #endregion targetObject_Defined_XPath

            #region targetObject_NOTDefined_XPath
            else // Object doesen't have xpath attribute
            {
                throw new MissingXPathException("Type T must define HasXPath attribute and include properties with XPath attribute.");
            }
            #endregion targetObject_NOTDefined_XPath
        }


        private static void ThrowIfNodeReturnTypeIsExplicitlySetWhenAttributeNameIsGiven(XPathAttribute xPathAttr)
        {
            if (xPathAttr.IsNodeReturnTypeExplicitlySet && !string.IsNullOrEmpty(xPathAttr.AttributeName))
            {
                throw new InvalidNodeReturnTypeException("Specifying a ReturnType value not allowed for XPathAttribute annotations targeting element attributes");
            }
        }
    }
}

