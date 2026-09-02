# Configurar la base de datos

La app guarda tu colección en una base de datos PostgreSQL propia (no hay ningún servidor de la app de por medio) y usa una cuenta de Supabase solo para el inicio de sesión y para sincronizar tus ajustes (cadena de conexión y claves de API) entre tus dispositivos.

## Si solo vas a usar la app (instalando el APK de una release)

No necesitas crear ningún proyecto Supabase — ya viene configurado en la app. Solo hace falta tu propia base de datos:

1. Crea una cuenta en [neon.tech](https://neon.tech) y un proyecto nuevo (elige la región más cercana). El plan gratuito permanente sobra para una colección personal.
2. No hace falta activar nada más (ni "Neon Auth" ni ninguna otra opción de backend) — solo la base de datos Postgres.
3. En el dashboard del proyecto, pestaña **Connection string**, copia la cadena en formato **.NET/Npgsql**, o adapta la estándar (`postgresql://usuario:contraseña@host/basededatos?sslmode=require`) a este formato:
   ```
   Host=<host>;Database=<basededatos>;Username=<usuario>;Password=<contraseña>;SSL Mode=Require
   ```
4. Pégala en la pantalla de Ajustes de la app (aparece sola en el primer inicio de sesión si tu cuenta aún no tiene una guardada) → GUARDAR Y CONECTAR. La app crea el esquema automáticamente la primera vez.

La cadena de conexión se guarda cifrada en tu cuenta de Supabase (protegida por Row Level Security: solo tú puedes leerla) y en `SecureStorage` del propio dispositivo como caché local.

## Si vas a compilar la app tú mismo desde el código

Además del paso anterior, necesitas **tu propio proyecto Supabase** (el del autor no está en el repositorio, ver `SupabaseConfig.cs.example`):

1. Crea una cuenta y un proyecto en [supabase.com](https://supabase.com) (plan gratuito).
2. En **Project Settings → API**, copia la **Project URL** y la clave **anon public** (es segura de embeber en la app: Row Level Security es lo que protege los datos, no el secreto de esta clave).
3. Copia `Infrastructure/Accounts/SupabaseConfig.cs.example` a `Infrastructure/Accounts/SupabaseConfig.cs` y pega ahí esos dos valores.
4. En el **SQL Editor** de Supabase, crea la tabla `user_settings` y su política de seguridad:
   ```sql
   create table public.user_settings (
     user_id uuid primary key references auth.users (id) on delete cascade,
     connection_string text not null default '',
     scan_dex_token text not null default '',
     igdb_client_id text not null default '',
     igdb_client_secret text not null default '',
     rawg_api_key text not null default '',
     thegamesdb_api_key text not null default ''
   );

   alter table public.user_settings enable row level security;

   create policy "Cada usuario ve y edita solo su propia fila"
     on public.user_settings
     for all
     using (auth.uid() = user_id)
     with check (auth.uid() = user_id);
   ```
5. Por defecto, Supabase pide confirmar el email al registrarse. Puedes desactivarlo en **Authentication → Providers → Email** si quieres probar la app más rápido durante el desarrollo.

Con esto, registrarte desde la app ya crea la fila en `user_settings` automáticamente al primer guardado de Ajustes.
