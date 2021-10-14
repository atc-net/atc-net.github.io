﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Extensions
{
    public static class NavigationManagerExtensions
    {
        /// <summary>
        /// Determines if the current page is the base page
        /// </summary>
        public static bool IsHomePage(this NavigationManager navMan)
        {
            return navMan.Uri == navMan.BaseUri;
        }

        /// <summary>
        /// Gets the section part of the documentation page
        /// </summary>
        public static string GetSection(this NavigationManager navMan)
        {
            // get the absolute path with out the base path
            var currentUri = navMan.Uri
                .Remove(0, navMan.BaseUri.Length - 1);

            var firstElement = currentUri
                .Split("/", StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return firstElement;
        }

        /// <summary>
        /// Gets the link of the component on the documentation page
        /// </summary>
        public static string GetComponentLink(this NavigationManager navMan)
        {
            // get the absolute path with out the base path
            var currentUri = navMan.Uri
                .Remove(0, navMan.BaseUri.Length - 1);

            var secondElement = currentUri
                .Split("/", StringSplitOptions.RemoveEmptyEntries)
                .ElementAtOrDefault(1);

            return secondElement;
        }
    }
}