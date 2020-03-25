using System;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using Diadem.Workflow.Core.Common;

namespace Diadem.Core
{
    public static class Guard
    {
        private const string ExceptionMessagesArgumentMustBeInterface = "Type must be an interface.";
        private const string ExceptionMessagesArgumentMustNotBeInterface = "Type must not be an interface.";

        private const string ExceptionMessagesArgumentMustNotBeNull = "Argument must not be null.";
        private const string ExceptionMessagesArgumentMustBeNull = "Argument must be null.";

        private const string ExceptionMessagesArgumentIsTrue = "Argument must be true.";
        private const string ExceptionMessagesArgumentIsFalse = "Argument must be false.";

        private const string ExceptionMessagesArgumentIsGreaterThan = "Argument must be greater than {0}.";
        private const string ExceptionMessagesArgumentIsGreaterOrEqual = "Argument must be greater or equal to {0}.";
        private const string ExceptionMessagesArgumentIsLowerThan = "Argument must be lower than {0}.";
        private const string ExceptionMessagesArgumentIsLowerOrEqual = "Argument must be lower or equal to {0}.";
        private const string ExceptionMessagesArgumentIsBetween = "Argument must be between {0}{1} and {2}{3}.";
        private const string ExceptionMessagesArgumentIsNotNegative = "Provided number must not be negative.";

        private const string ExceptionMessagesArgumentMustNotBeEmpty = "Argument must not be empty.";
        private const string ExceptionMessagesArgumentHasLength = "Expected string length is {0}, but found {1}.";

        private const string ExceptionMessagesArgumentHasMaxLength =
            "String length exceeds maximum of {0} characters. Found string of length {1}.";

        private const string ExceptionMessagesArgumentHasMinLength =
            "String must have a minimum of {0} characters. Found string of length {1}.";

        private const string ExceptionMessagesArgumentCondition = "Given condition \"{0}\" is not met.";

        /// <summary>
        ///     Checks if the given <paramref name="expression" /> is true.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="expression" /> parameter is false.</exception>
        public static void ArgumentIsTrue([ValidatedNotNull] Expression<Func<bool>> expression)
        {
            ArgumentIsTrueOrFalse(expression, false, ExceptionMessagesArgumentIsFalse);
        }

        /// <summary>
        ///     Checks if the given <paramref name="expression" /> is true.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="expression" /> parameter is false.</exception>
        public static void ArgumentIsTrue([ValidatedNotNull] Expression<Func<bool>> expression, [ValidatedNotNull] string message)
        {
            ArgumentIsTrueOrFalse(expression, false, message);
        }

        /// <summary>
        ///     Checks if the given <paramref name="expression" /> is false.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="expression" /> parameter is true.</exception>
        public static void ArgumentIsFalse([ValidatedNotNull] Expression<Func<bool>> expression)
        {
            ArgumentIsTrueOrFalse(expression, true, ExceptionMessagesArgumentIsTrue);
        }

        /// <summary>
        ///     Checks if the given <paramref name="expression" /> is false.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="expression" /> parameter is true.</exception>
        public static void ArgumentIsFalse([ValidatedNotNull] Expression<Func<bool>> expression, [ValidatedNotNull] string message)
        {
            ArgumentIsTrueOrFalse(expression, true, message);
        }

        /// <summary>
        ///     Checks if the given value meets the given condition.
        /// </summary>
        /// <example>
        ///     Only pass single parameters through to this call via expression, e.g. Guard.ArgumentCondition(() => value, v =>
        ///     true)
        /// </example>
        public static void ArgumentCondition<T>([ValidatedNotNull] Expression<Func<T>> expression,
            Expression<Func<T, bool>> condition)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentCondition(propertyValue, paramName, condition);
        }

        /// <summary>
        ///     Checks if the given value meets the given condition.
        /// </summary>
        /// <example>
        ///     Only pass single parameters through to this call via expression, e.g. Guard.ArgumentCondition(value, "value", v =>
        ///     true)
        /// </example>
        public static void ArgumentCondition<T>([ValidatedNotNull] T value, string paramName,
            Expression<Func<T, bool>> condition)
        {
            ArgumentNotNull(condition, paramName);

            if (!condition.Compile()(value))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentCondition, condition), paramName);
            }
        }

        /// <summary>
        ///     Checks if the given string is not null or empty.
        /// </summary>
        public static void ArgumentNotNullOrEmpty([ValidatedNotNull] Expression<Func<IEnumerable>> expression)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentNotNullOrEmpty(propertyValue, paramName);
        }

        /// <summary>
        ///     Checks if the given string is not null or empty.
        /// </summary>
        public static void ArgumentNotNullOrEmpty([ValidatedNotNull] IEnumerable enumerable, string paramName)
        {
            ArgumentNotNull(enumerable, paramName);

            var hasElement = enumerable.GetEnumerator().MoveNext();
            if (!hasElement)
            {
                throw new ArgumentException(ExceptionMessagesArgumentMustNotBeEmpty, paramName);
            }
        }

        /// <summary>
        ///     Checks if the given value is not null.
        /// </summary>
        /// <example>
        ///     Only pass single parameters through to this call via expression, e.g. Guard.ArgumentNull(() => someParam)
        /// </example>
        /// <param name="expression">An expression containing a single string parameter e.g. () => someParam</param>
        public static void ArgumentNull<T>([ValidatedNotNull] Expression<Func<T>> expression)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentNull(propertyValue, paramName);
        }

        /// <summary>
        ///     Checks if the given value is not null.
        /// </summary>
        /// <example>
        ///     Only pass single parameters through to this call via expression, e.g. Guard.ArgumentNull(value, "value")
        /// </example>
        public static void ArgumentNull<T>([ValidatedNotNull] T value, string paramName)
        {
            if (value != null)
            {
                throw new ArgumentException(ExceptionMessagesArgumentMustBeNull, paramName);
            }
        }

        /// <summary>
        ///     Checks if the given value is not null.
        /// </summary>
        /// <example>
        ///     Only pass single parameters through to this call via expression, e.g. Guard.ArgumentNotNull(() => someParam)
        /// </example>
        /// <param name="expression">An expression containing a single string parameter e.g. () => someParam</param>
        public static void ArgumentNotNull<T>([ValidatedNotNull] Expression<Func<T>> expression)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentNotNull(propertyValue, paramName);
        }

        /// <summary>
        ///     Checks if the given value is not null.
        /// </summary>
        /// <example>
        ///     Only pass single parameters through to this call via expression, e.g. Guard.ArgumentNotNull(value, "value")
        /// </example>
        public static void ArgumentNotNull<T>([ValidatedNotNull] T value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, ExceptionMessagesArgumentMustNotBeNull);
            }
        }

        /// <summary>
        ///     Checks if given argument is greater than given value.
        /// </summary>
        /// <param name="expression">Given argument</param>
        /// <param name="givenValue">Given value.</param>
        public static void ArgumentIsGreaterThan<T>([ValidatedNotNull] Expression<Func<T>> expression, T givenValue)
            where T : struct, IComparable<T>
        {
            ArgumentNotNull(expression);

            var propertyValue = expression.Compile()();
            if (propertyValue.IsLessThanOrEqual(givenValue))
            {
                var paramName = expression.GetMemberName();
                throw new ArgumentOutOfRangeException(paramName, propertyValue,
                    string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentIsGreaterThan, givenValue));
            }
        }

        /// <summary>
        ///     Checks if given argument is greater or equal to given value.
        /// </summary>
        /// <param name="argument">Given argument</param>
        /// <param name="givenValue">Given value.</param>
        public static void ArgumentIsGreaterOrEqual<T>([ValidatedNotNull] Expression<Func<T>> argument, T givenValue)
            where T : struct, IComparable<T>
        {
            ArgumentNotNull(argument);

            var propertyValue = argument.Compile()();
            if (propertyValue.IsLessThan(givenValue))
            {
                var paramName = ((MemberExpression) argument.Body).Member.Name;
                throw new ArgumentOutOfRangeException(paramName, propertyValue,
                    string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentIsGreaterOrEqual, givenValue));
            }
        }

        /// <summary>
        ///     Checks if given argument is lower than given value.
        /// </summary>
        /// <param name="argument">Given argument</param>
        /// <param name="givenValue">Given value.</param>
        public static void ArgumentIsLowerThan<T>([ValidatedNotNull] Expression<Func<T>> argument, T givenValue)
            where T : struct, IComparable<T>
        {
            ArgumentNotNull(argument);

            var propertyValue = argument.Compile()();
            if (propertyValue.IsGreaterOrEqual(givenValue))
            {
                var paramName = ((MemberExpression) argument.Body).Member.Name;
                throw new ArgumentOutOfRangeException(paramName, propertyValue,
                    string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentIsLowerThan, givenValue));
            }
        }

        /// <summary>
        ///     Checks if given argument is lower or equal to given value.
        /// </summary>
        /// <param name="argument">Given argument</param>
        /// <param name="givenValue">Given value.</param>
        public static void ArgumentIsLowerOrEqual<T>([ValidatedNotNull] Expression<Func<T>> argument, T givenValue)
            where T : struct, IComparable<T>
        {
            ArgumentNotNull(argument);

            var propertyValue = argument.Compile()();
            if (propertyValue.IsGreaterThan(givenValue))
            {
                var paramName = ((MemberExpression) argument.Body).Member.Name;
                throw new ArgumentOutOfRangeException(paramName, propertyValue,
                    string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentIsLowerOrEqual, givenValue));
            }
        }

        /// <summary>
        ///     Checks if given argument is between given lower value and upper value.
        /// </summary>
        /// <param name="argument">Given argument</param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="inclusive">
        ///     Inclusive lower bound value if
        ///     <param name="inclusive">true</param>
        ///     .
        /// </param>
        public static void ArgumentIsBetween<T>([ValidatedNotNull] Expression<Func<T>> argument, T lowerBound,
            T upperBound, bool inclusive = false) where T : struct, IComparable<T>
        {
            ArgumentNotNull(argument);

            var propertyValue = argument.Compile()();
            if (!propertyValue.IsBetween(lowerBound, upperBound, inclusive))
            {
                var paramName = ((MemberExpression) argument.Body).Member.Name;
                throw new ArgumentOutOfRangeException(paramName, propertyValue,
                    string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentIsBetween, inclusive ? "(" : "[", lowerBound, upperBound,
                        inclusive ? ")" : "]"));
            }
        }

        /// <summary>
        ///     Verifies the <paramref name="expression" /> is not a negative number and throws an
        ///     <see cref="ArgumentOutOfRangeException" /> if it is a negative number.
        /// </summary>
        /// <param name="expression">An expression containing a single parameter e.g. () => param</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="expression" /> parameter is a negative number.</exception>
        public static void ArgumentIsNotNegative<T>([ValidatedNotNull] Expression<Func<T>> expression)
            where T : struct, IComparable<T>
        {
            ArgumentNotNull(expression);

            var argumentValue = expression.Compile()();
            ArgumentIsNotNegative(argumentValue, expression.GetMemberName());
        }

        /// <summary>
        ///     Checks if <paramref name="argumentValue" /> is not a negative number.
        /// </summary>
        /// <param name="argumentValue">The value to verify.</param>
        /// <param name="argumentName">The name of the <paramref name="argumentValue" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="argumentValue" /> parameter is a negative number.</exception>
        public static void ArgumentIsNotNegative<T>(T argumentValue, string argumentName)
            where T : struct, IComparable<T>
        {
            if (argumentValue.IsLessThan(default))
            {
                throw new ArgumentOutOfRangeException(argumentName, argumentValue,
                    string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentIsNotNegative));
            }
        }

        /// <summary>
        ///     Checks if the given <paramref name="type" /> is an interface type.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="type" /> parameter is not an interface type.</exception>
        public static void ArgumentMustBeInterface([ValidatedNotNull] Type type)
        {
            CheckIfTypeIsInterface(type, false, ExceptionMessagesArgumentMustBeInterface);
        }

        /// <summary>
        ///     Checks if the given <paramref name="type" /> is not an interface type.
        /// </summary>
        /// <exception cref="ArgumentException">The <paramref name="type" /> parameter is an interface type.</exception>
        public static void ArgumentMustNotBeInterface([ValidatedNotNull] Type type)
        {
            CheckIfTypeIsInterface(type, true, ExceptionMessagesArgumentMustNotBeInterface);
        }

        private static void CheckIfTypeIsInterface(Type type, bool throwIfItIsAnInterface, string exceptionMessage)
        {
            ArgumentNotNull(type, "type");

            if (type.IsInterface == throwIfItIsAnInterface)
            {
                throw new ArgumentException(exceptionMessage, type.Name);
            }
        }

        /// <summary>
        ///     Checks if the given string is not null or empty.
        /// </summary>
        public static void ArgumentNotNullOrEmpty([ValidatedNotNull] Expression<Func<string>> expression)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentNotNullOrEmpty(propertyValue, paramName);
        }

        /// <summary>
        ///     Checks if the given string is not null or empty.
        /// </summary>
        public static void ArgumentNotNullOrEmpty([ValidatedNotNull] string propertyValue, string paramName)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                ArgumentNotNull(propertyValue, paramName);

                throw new ArgumentException(ExceptionMessagesArgumentMustNotBeEmpty, paramName);
            }
        }

        /// <summary>
        ///     Checks if the given string has the expected length
        /// </summary>
        /// <param name="expression">Property expression.</param>
        /// <param name="expectedLength">Expected length.</param>
        public static void ArgumentHasLength([ValidatedNotNull] Expression<Func<string>> expression, int expectedLength)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var length = propertyValue.Length;
            if (length != expectedLength)
            {
                var paramName = expression.GetMemberName();
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentHasLength, expectedLength, length),
                    paramName);
            }
        }

        /// <summary>
        ///     Checks if the given string has the expected length
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="expectedLength">Expected length.</param>
        public static void ArgumentHasLength([ValidatedNotNull] string propertyValue, string paramName,
            int expectedLength)
        {
            ArgumentNotNull(propertyValue, paramName);

            var length = propertyValue.Length;
            if (length != expectedLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentHasLength, expectedLength, length),
                    paramName);
            }
        }

        /// <summary>
        ///     Checks if the given string has a length which exceeds given max length.
        /// </summary>
        public static void ArgumentHasMaxLength([ValidatedNotNull] Expression<Func<string>> expression, int maxLength)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentHasMaxLength(propertyValue, paramName, maxLength);
        }

        /// <summary>
        ///     Checks if the given string has a length which exceeds given max length.
        /// </summary>
        public static void ArgumentHasMaxLength([ValidatedNotNull] string propertyValue, string paramName,
            int maxLength)
        {
            ArgumentNotNull(propertyValue, paramName);

            var length = propertyValue.Length;
            if (length > maxLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentHasMaxLength, maxLength, length),
                    paramName);
            }
        }

        /// <summary>
        ///     Checks if the given string has a length which is at least given min length long.
        /// </summary>
        public static void ArgumentHasMinLength([ValidatedNotNull] Expression<Func<string>> expression, int minLength)
        {
            ArgumentNotNull(expression, nameof(expression));

            var propertyValue = expression.Compile()();
            var paramName = expression.GetMemberName();

            ArgumentHasMinLength(propertyValue, paramName, minLength);
        }

        /// <summary>
        ///     Checks if the given string has a length which is at least given min length long.
        /// </summary>
        public static void ArgumentHasMinLength([ValidatedNotNull] string propertyValue, string paramName,
            int minLength)
        {
            ArgumentNotNull(propertyValue, paramName);

            var length = propertyValue.Length;
            if (length < minLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessagesArgumentHasMinLength, minLength, length),
                    paramName);
            }
        }

        private static void ArgumentIsTrueOrFalse(Expression<Func<bool>> expression, bool throwCondition,
            string exceptionMessage)
        {
            ArgumentNotNull(expression, nameof(expression));

            if (expression.Compile().Invoke() == throwCondition)
            {
                var paramName = expression.GetMemberName();
                throw new ArgumentException(exceptionMessage, paramName);
            }
        }
    }
}