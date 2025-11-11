# 📚 NLP Exam Generator

Un generador de exámenes inteligente que utiliza **Procesamiento de Lenguaje Natural (NLP)** e **Inteligencia Artificial** para crear preguntas de opción múltiple a partir de documentos PDF y texto.

## ✨ Características

- 📄 **Extracción de texto de PDFs**: Sube documentos PDF y extrae automáticamente el contenido
- 🤖 **Generación automática de preguntas**: Utiliza OpenAI GPT para crear preguntas inteligentes
- 📊 **Exámenes personalizados**: Genera múltiples preguntas con opciones múltiples
- 📱 **Interfaz web moderna**: Diseño responsivo con Bootstrap
- 🔐 **Autenticación de usuarios**: Sistema de login y registro
- 💾 **Persistencia de datos**: Guarda exámenes y preguntas en base de datos
- 📋 **Exportación a PDF**: Genera PDFs de los exámenes creados
- 🌐 **Multiplataforma**: Compatible con Windows y Linux

## 🏗️ Arquitectura

El proyecto sigue una **arquitectura en capas** con las siguientes estructuras:

```
NLPExamGenerator/
├── NLPExamGenerator.Entidades/     # Modelos de datos y Entity Framework
├── NLPExamGenerator.Logica/        # Lógica de negocio
└── NLPExamGenerator.WebApp/        # Aplicación web ASP.NET Core MVC
```

### Tecnologías Utilizadas

- **Backend**: ASP.NET Core 8.0 MVC
- **Base de Datos**: SQL Server / SQLite (multiplataforma)
- **ORM**: Entity Framework Core
- **IA**: OpenAI GPT API
- **PDF**: QuestPDF para generación, bibliotecas de extracción de texto
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Autenticación**: ASP.NET Core Identity con Cookies

## 📖 Uso

### 1. Registro e Inicio de Sesión
- Crear una cuenta nueva o iniciar sesión
- El sistema mantiene la sesión con cookies

### 2. Generar Examen
1. Subir un archivo PDF o pegar texto directamente
2. Especificar el número de preguntas deseadas
3. El sistema extrae el texto y lo envía a OpenAI
4. Se generan preguntas de opción múltiple con explicaciones

### 3. Gestionar Exámenes
- Ver todos tus exámenes creados
- Revisar preguntas y respuestas
- Exportar exámenes a PDF

## 🌟 Características Avanzadas

### Compatibilidad Multiplataforma
El sistema detecta automáticamente el SO y utiliza:
- **Windows**: SQL Server LocalDB
- **Linux**: SQLite

### Limitaciones Inteligentes
- Texto limitado a 60,000 caracteres para optimizar las consultas a OpenAI
- Manejo de errores y validaciones robustas
- Sesiones con timeout configurable

### Generación de PDFs
- Diseño profesional con QuestPDF
- Incluye preguntas, opciones y explicaciones
- Exportación rápida y eficiente

## 📄 Licencia

Este proyecto está desarrollado como parte de un trabajo académico para **Programación Web III** en **UNLAM**.

---

**¡Transforma tus documentos en exámenes inteligentes con NLP Exam Generator!** 🎓✨