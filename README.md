# ConsChatGPT

Not even good implementation of ChatGPT in C# in console.

## How to use

1. Install .NET 7
2. Clone repository using Git or download sources in ZIP archive.
3. Edit `OPENAI_API_TOKEN` constant in `Program.cs`: add you OpenAI token from [this page](https://platform.openai.com/account/api-keys).
4. Run `dotnet run` in sources directory.
5. Write your messages, get your answers. Additional commands: /system to add system message, /clearctx to clear context (messages history), /regenerate to regenerate answer.

## License

Project is in the public domain. Do what you want with it.

Thanks to Eugene Popov ([metanit.com](https://metanit.com/sharp/libs/3.1.php)) for inspiration and code base for this project.
