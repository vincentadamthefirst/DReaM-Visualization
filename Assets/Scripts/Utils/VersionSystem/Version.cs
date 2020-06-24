using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Utils.VersionSystem {
    public class Version : IComparable {

        private List<int> _versionPartsInt;
        private readonly string _versionAppendix = "";
        private readonly string _versionString;

        private const string VersionPatternNoAppendix = @"^(\d+\.)+(\d+)$";
        private const string VersionPatternAppendix = @"^(\d+\.)+(\d+)[a-z]+$";

        /// <summary>
        /// Takes in a string and tries to convert it into a version.
        /// Version strings must follow these rules:
        /// - starting with a number
        /// - containing numbers, separated by dots
        /// - ending on a string, which must not be separated by a dot from the last number
        /// - ending on a number
        /// </summary>
        /// <param name="versionString">The version string to be parsed.</param>
        /// <exception cref="VersionParseException">When the string does not match the pattern.</exception>
        public Version(string versionString) {
            _versionString = versionString.ToLower();

            if (Regex.IsMatch(_versionString, VersionPatternNoAppendix)) {
                ParseNumbers(_versionString);
            } else if (Regex.IsMatch(_versionString, VersionPatternAppendix)) {
                var numberPart = new string(_versionString.TakeWhile(c => char.IsDigit(c) || char.IsPunctuation(c))
                    .ToArray());
                var appendixPart =
                    new string(_versionString.Where(c => char.IsLetter(c) && !char.IsPunctuation(c)).ToArray());

                _versionAppendix = appendixPart;
                ParseNumbers(numberPart);
            } else {
                throw new VersionParseException(
                    "Version string can not be parsed as it does not follow the version pattern rules.");
            }
        }

        /// <summary>
        /// Parses a string consisting only of numbers separated by dots into the internal List of version parts.
        /// </summary>
        /// <param name="numberString">The string to be parsed.</param>
        private void ParseNumbers(string numberString) {
            _versionPartsInt = new List<int>();
            var splitString = numberString.Split(new[] {"."}, StringSplitOptions.None);
            foreach (var s in splitString) {
                _versionPartsInt.Add(int.Parse(s));
            }
        }

        /// <summary>
        /// Returns the version as a string.
        /// </summary>
        /// <returns>The version string.</returns>
        public string GetString() {
            return _versionString;
        }

        /// <summary>
        /// Returns the length of the number part of the version.
        /// </summary>
        /// <returns>The length of the number part.</returns>
        private int GetLength() {
            return _versionPartsInt.Count;
        }

        /// <summary>
        /// Returns a specific number of the version given by an index. If the id does not exist this method will return
        /// -1.
        /// </summary>
        /// <param name="index">The id of the part of the version.</param>
        /// <returns>The number of the version at the given index.</returns>
        private int GetVersionPart(int index) {
            if (index > _versionPartsInt.Count - 1 || index < 0) return -1;
            return _versionPartsInt[index];
        }

        /// <summary>
        /// Returns the appendix of the version, might be an empty string.
        /// </summary>
        /// <returns>The appendix of the version.</returns>
        private string GetAppendix() {
            return _versionAppendix;
        }

        /// <summary>
        /// Compares two versions. Will take numbers and the appendix into consideration, the version do not need to
        /// have the same length.
        /// </summary>
        /// <param name="obj">The other version.</param>
        /// <returns>-1 if obj is greater, 0 if they are the same, 1 if obj is smaller</returns>
        /// <exception cref="ArgumentException">When the obj is not a Version.</exception>
        public int CompareTo(object obj) {
            switch (obj) {
                case null:
                    return 1;
                case Version otherVersion: {
                    var maxLength = GetLength() > otherVersion.GetLength() ? GetLength() : otherVersion.GetLength();
                    for (var i = 0; i < maxLength; i++) {
                        if (GetVersionPart(i) > otherVersion.GetVersionPart(i)) return 1;
                        if (GetVersionPart(i) < otherVersion.GetVersionPart(i)) return -1;
                    }

                    return string.Compare(_versionAppendix, otherVersion.GetAppendix(), StringComparison.Ordinal);
                }
                default:
                    throw new ArgumentException("Object is not a Version!");
            }
        }
    }

    [Serializable]
    public class VersionParseException : Exception {
        public VersionParseException() { }
        public VersionParseException(string message) : base(message) { }
        public VersionParseException(string message, Exception inner) : base(message, inner) { }

        protected VersionParseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}