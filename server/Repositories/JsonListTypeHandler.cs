using System.Data;
using System.Text.Json;
using Dapper;

namespace SmartCv.Repositories;

public class JsonListTypeHandler : SqlMapper.TypeHandler<List<string>>
{
    public override void SetValue(IDbDataParameter parameter, List<string>? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = JsonSerializer.Serialize(value ?? []);
    }

    public override List<string> Parse(object value) =>
        value is string text && text.Length > 0
            ? JsonSerializer.Deserialize<List<string>>(text) ?? []
            : [];
}
