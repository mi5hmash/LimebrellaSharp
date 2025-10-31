﻿using System.Diagnostics;

namespace LimebrellaSharpCore.Infrastructure;

/// <summary>
/// Provides constants and utility methods for accessing project-related URLs and opening them in the default web browser.
/// </summary>
public static class Urls
{
    public const string AuthorsGithub = "https://github.com/mi5hmash";
    public const string ProjectsRepo = "https://github.com/mi5hmash/LimebrellaSharp";

    public static void OpenAuthorsGithub() => OpenUrl(AuthorsGithub);
    public static void OpenProjectsRepo() => OpenUrl(ProjectsRepo);
    
    /// <summary>
    /// Opens the given URL in the default web browser.
    /// </summary>
    /// <param name="url"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static void OpenUrl(this string url)
    {
        if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            Process.Start("xdg-open", url);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", url);
        else
            throw new PlatformNotSupportedException("Unsupported OS platform.");
    }
}