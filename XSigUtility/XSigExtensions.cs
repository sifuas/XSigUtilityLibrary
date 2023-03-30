using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Crestron.SimplSharp.CrestronIO;

using XSigUtilityLibrary.Serialization;
using XSigUtilityLibrary.Tokens;

namespace XSigUtilityLibrary
{
    public static class XSigExtensions
    {
        /// <summary>
        /// Get bytes for an IXSigStateResolver object.
        /// </summary>
        /// <param name="serializer">XSig state resolver.</param>
        /// <param name="obj">Object to serialize as bytes.</param>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns>Bytes in XSig format for each token within the state representation.</returns>
        /// <exception cref="ArgumentNullException">Object or XSig serializer is null.</exception>
        public static byte[] GetBytes<T>(this IXSigSerializer<T> serializer, T obj)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            return serializer.GetBytes(obj, 0);
        }

        /// <summary>
        /// Get bytes for an IXSigStateResolver object, with a specified offset.
        /// </summary>
        /// <param name="serializer">XSig state resolver.</param>
        /// <param name="obj">Object to serialize as bytes.</param>
        /// <param name="offset">Offset to which the data will be aligned.</param>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns>Bytes in XSig format for each token within the state representation.</returns>
        /// <exception cref="ArgumentNullException">Object or XSig serializer is null.</exception>
        public static byte[] GetBytes<T>(this IXSigSerializer<T> serializer, T obj, int offset)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var tokens = serializer.Serialize(obj);
            if (tokens == null) return new byte[0];
            using (var memoryStream = new MemoryStream())
            {
                using (var tokenWriter = new XSigTokenStreamWriter(memoryStream))
                    tokenWriter.SerializeToStream(obj, serializer, offset);

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize object from stream.
        /// </summary>
        /// <param name="reader">XSig token stream reader.</param>
        /// <param name="serializer">XSig serializer.</param>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns>Deserialized object.</returns>
        /// <exception cref="ArgumentNullException">Object or XSig serializer is null.</exception>
        public static T DeserializeFromStream<T>(this XSigTokenStreamReader reader, IXSigSerializer<T> serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            return serializer.Deserialize(reader.ReadAllXSigTokens());
        }

        /// <summary>
        /// Write the serialized XSig object to the stream.
        /// </summary>
        /// <param name="writer">XSig token stream writer.</param>
        /// <param name="obj">Object to serialize to the stream.</param>
        /// <param name="serializer">XSig serializer.</param>
        /// <typeparam name="T">Object type.</typeparam>
        /// <exception cref="ArgumentNullException">Object or XSig serializer is null.</exception>
        public static void SerializeToStream<T>(this XSigTokenStreamWriter writer, T obj, IXSigSerializer<T> serializer)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            writer.SerializeToStream(obj, serializer, 0);
        }

        /// <summary>
        /// Write the serialized XSig object to the stream.
        /// </summary>
        /// <param name="writer">XSig token stream writer.</param>
        /// <param name="obj">Object to serialize to the stream.</param>
        /// <param name="serializer">XSig serializer.</param>
        /// <param name="offset"></param>
        /// <typeparam name="T">Object type.</typeparam>
        /// <exception cref="ArgumentNullException">Object or XSig serializer is null.</exception>
        public static void SerializeToStream<T>(this XSigTokenStreamWriter writer, T obj, IXSigSerializer<T> serializer, int offset)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var tokens = serializer.Serialize(obj);
            writer.WriteXSigData(tokens, offset);
        }

        public static readonly int XSigEncoding = 28591;

        /// <summary>
        /// Return the given XSigToken array as the string representation for Simpl.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string AsXSigString(this XSigToken token)
        {
            if (token == null)
                return "";

            string returnString;
            using (var s = new MemoryStream())
            {
                using (var tw = new XSigTokenStreamWriter(s, true))
                {
                    tw.WriteXSigData(token);
                }

                var xSig = s.ToArray();

                returnString = Encoding.GetEncoding(XSigEncoding).GetString(xSig, 0, xSig.Length);
            }

            return returnString;
        }

        /// <summary>
        /// Return the given XSigToken array as the string representation for Simpl.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string AsXSigString(this XSigToken[] tokens)
        {
            if (tokens == null || tokens.Length == 0)
                return "";

            string returnString;
            using (var s = new MemoryStream())
            {
                using (var tw = new XSigTokenStreamWriter(s, true))
                {
                    tw.WriteXSigData(tokens);
                }

                var xSig = s.ToArray();

                returnString = Encoding.GetEncoding(XSigEncoding).GetString(xSig, 0, xSig.Length);
            }

            return returnString;
        }

        /// <summary>
        /// Return the given XSigToken ICollection as the string representation for Simpl.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string AsXSigString(this ICollection<XSigToken> tokens)
        {
            if (tokens != null)
                return tokens.ToArray<XSigToken>().AsXSigString();
            else
                return "";
        }

        /// <summary>
        /// Return the given XSigToken IEnumerable as the string representation for Simpl.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string AsXSigString(this IEnumerable<XSigToken> tokens)
        {
            if (tokens != null)
                return tokens.ToArray<XSigToken>().AsXSigString();
            else
                return "";

        }

        /// <summary>
        /// Return the given byte array representation of an XSigToken as the string representation.
        /// </summary>
        /// <param name="xSigByteValue"></param>
        /// <returns></returns>
        public static string AsXSigString(this byte[] xSigByteValues)
        {
            return Encoding.GetEncoding(XSigEncoding).GetString(xSigByteValues, 0, xSigByteValues.Length);
        }

        /// <summary>
        /// Return the given string as the XSig representation of a serial signal for Simpl.
        /// </summary>
        /// <param name="?"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string AsXSigString(this string value, int index)
        {
            return new XSigSerialToken(index, value).AsXSigString();
        }

        /// <summary>
        /// Return the given ushort as the XSig representation of an analog signal for Simpl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string AsXSigString(this ushort value, int index)
        {
            return new XSigAnalogToken(index, value).AsXSigString();
        }

        /// <summary>
        /// Return the given bool as the XSig representation of a digital signal for Simpl.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string AsXSigString(this bool value, int index)
        {
            return new XSigDigitalToken(index, value).AsXSigString();
        }
    }
}