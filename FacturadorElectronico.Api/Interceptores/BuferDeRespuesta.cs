namespace FacturadorElectronico.Api.Interceptores
{
    public static class BuferDeRespuesta
    {
        private const string FeatureKey = "__shared_response_buffer__";

        private sealed class SharedBuffer
        {
            public int RefCount;
            public Stream OriginalBody = default!;
            public MemoryStream Buffer = default!;
            public bool Flushed;
        }

        /// <summary>
        /// Método para comenzar a almacenar en búfer el cuerpo de la respuesta en el HttpContext.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task<IAsyncDisposable> BeginAsync(HttpContext ctx)
        {
            ArgumentNullException.ThrowIfNull(ctx);

            IDictionary<object, object?> features = ctx.Items;
            if (!features.TryGetValue(FeatureKey, out object? obj) || obj is not SharedBuffer shared)
            {
                shared = new SharedBuffer
                {
                    RefCount = 0,
                    OriginalBody = ctx.Response.Body,
                    Buffer = new MemoryStream(),
                    Flushed = false
                };
                features[FeatureKey] = shared;
                ctx.Response.Body = shared.Buffer;

                ctx.Response.OnCompleted(() =>
                {
                    try { return FlushAndRestoreIfNeeded(ctx, shared); }
                    catch { return Task.CompletedTask; }
                });
            }

            shared.RefCount++;

            return new AsyncDisposable(async () =>
            {
                shared.RefCount--;

                if (shared.RefCount == 0 && !shared.Flushed)
                {
                    await FlushAndRestoreIfNeeded(ctx, shared);
                }
            });
        }

        /// <summary>
        /// Método para obtener el cuerpo de la respuesta como una cadena de texto.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetBodyAsString(HttpContext ctx)
        {
            SharedBuffer shared = GetShared(ctx);
            shared.Buffer.Position = 0;
            using StreamReader reader = new StreamReader(shared.Buffer, leaveOpen: true);
            string text = reader.ReadToEnd();
            shared.Buffer.Position = 0;
            return text;
        }

        /// <summary>
        /// Método para obtener el SharedBuffer del HttpContext.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static SharedBuffer GetShared(HttpContext ctx)
        {
            if (!ctx.Items.TryGetValue(FeatureKey, out object? obj) || obj is not SharedBuffer shared)
                throw new InvalidOperationException("ResponseBuffering no inicializado. Llama a BeginAsync primero.");
            return shared;
        }

        /// <summary>
        /// Método para vaciar el búfer y restaurar el cuerpo original de la respuesta si es necesario.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="shared"></param>
        /// <returns></returns>
        private static async Task FlushAndRestoreIfNeeded(HttpContext ctx, SharedBuffer shared)
        {
            if (shared.Flushed) return;

            shared.Buffer.Position = 0;

            await shared.Buffer.CopyToAsync(shared.OriginalBody, ctx.RequestAborted);

            ctx.Response.Body = shared.OriginalBody;
            shared.Flushed = true;

            shared.Buffer.Dispose();
        }

        /// <summary>
        /// Método para limpiar el búfer de respuesta y restaurar el cuerpo original.
        /// </summary>
        private sealed class AsyncDisposable : IAsyncDisposable
        {
            private readonly Func<Task> _dispose;
            public AsyncDisposable(Func<Task> dispose) => _dispose = dispose;
            public ValueTask DisposeAsync() => new(_dispose());
        }
    }

}
