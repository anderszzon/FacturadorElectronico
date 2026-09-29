using System.Reflection;
using System.Text.RegularExpressions;

namespace FacturadorElectronico.Api.Extensiones
{
    public static class ExtensionesVersionamiento
    {
        private sealed class Candidato(MethodInfo metodo, string namespaceDeclarante, int? version)
        {
            public MethodInfo Metodo { get; } = metodo;
            public string NamespaceDeclarante { get; } = namespaceDeclarante;
            public int? Version { get; } = version;
        }

        public static RouteGroupBuilder MapearTodasLasVersionesControlador(
            this RouteGroupBuilder apiGrupo,
            string nombreControlador
        )
        {
            ArgumentNullException.ThrowIfNull(apiGrupo);

            Assembly ensamblado = Assembly.GetExecutingAssembly();
            Regex expresionVersion = new(@"\.V(?<num>\d+)(\.|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            List<Candidato> candidatos = ensamblado
                .GetTypes()
                .Where(tipo =>
                    tipo.IsSealed && tipo.IsAbstract &&
                    tipo.Namespace != null &&
                    tipo.Namespace.StartsWith("FacturadorElectronico.Api.Controladores.", StringComparison.Ordinal))
                .SelectMany(tipo => tipo.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(metodo =>
                    string.Equals(metodo.Name, $"Map{nombreControlador}", StringComparison.Ordinal) &&
                    metodo.ReturnType == typeof(RouteGroupBuilder))
                .Select(metodo =>
                {
                    Type? tipoDeclarante = metodo.DeclaringType;
                    string espacioNombres = tipoDeclarante != null && tipoDeclarante.Namespace != null
                        ? tipoDeclarante.Namespace
                        : string.Empty;

                    int? version = ExtraerVersion(espacioNombres, expresionVersion);
                    return new Candidato(metodo, espacioNombres, version);
                })
                .Where(candidato => candidato.Version.HasValue)
                .ToList();

            if (candidatos.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No se encontraron métodos Map'{nombreControlador}' en namespaces 'FacturadorElectronico.Controladores.VN'.");
            }

            foreach (Candidato candidato in candidatos.OrderBy(c => c.Version!.Value))
            {
                int version = candidato.Version!.Value;

                RouteGroupBuilder versionGrupo = apiGrupo.MapGroup($"/v{version}/{nombreControlador}");

                ParameterInfo[] parametros = candidato.Metodo.GetParameters();
                bool firmaEsperada = parametros.Length == 1 && parametros[0].ParameterType == typeof(RouteGroupBuilder);

                if (firmaEsperada)
                {
                    object? resultadoObj = candidato.Metodo.Invoke(null, [versionGrupo]);
                    RouteGroupBuilder? resultado = resultadoObj as RouteGroupBuilder;

                    if (resultado is null)
                    {
                        throw new InvalidOperationException(
                            $"El método {candidato.Metodo.DeclaringType!.FullName}.{candidato.Metodo.Name} devolvió null.");
                    }

                    resultado
                        .WithTags($"{nombreControlador} V{version}")
                        .WithMetadata(new EndpointNameMetadata($"{nombreControlador}V{version}"));

                }
                else
                {
                    throw new InvalidOperationException(
                        $"El método {candidato.Metodo.DeclaringType!.FullName}.{candidato.Metodo.Name} no tiene la firma esperada: " +
                        "RouteGroupBuilder MapX(this RouteGroupBuilder).");
                }
            }

            return apiGrupo;

            static int? ExtraerVersion(string cadenaNamespace, Regex regex)
            {
                Match coincidencia = regex.Match(cadenaNamespace);
                if (!coincidencia.Success) return null;

                int valorVersion;
                if (int.TryParse(coincidencia.Groups["num"].Value, out valorVersion)) return valorVersion;

                return null;
            }
        }
    }
}
