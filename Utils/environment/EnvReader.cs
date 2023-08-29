namespace Utils.environment
{

    public class EnvReader<T> where T : new()
    {
        public T Variables { get; } = new ();

        private static string ToSnakeCase(string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToUpper();
        }

        private string ReadVariable(string variableName)
        {
            var variableValue = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrEmpty(variableValue))
            {
                throw new Exception($"{variableName} is undefined");
            }

            return variableValue;
        }

        public EnvReader()
        {
            foreach (var property in typeof(T).GetProperties().Where(prop => prop.CanWrite))
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(Variables, ReadVariable(ToSnakeCase(property.Name)));
                }
                else if (property.PropertyType == typeof(int))
                {
                    string readValue = ReadVariable(ToSnakeCase(property.Name));
                    if (int.TryParse(readValue, out int number))
                    {
                        property.SetValue(Variables, number);
                    }
                    else
                    {
                        throw new Exception($"Cannot convert {ToSnakeCase(property.Name)}: {readValue} to int");
                    }
                }
                else
                {
                    throw new Exception($"Cannot read {property.PropertyType} {property.Name}");
                }
            }
        }
    }
}