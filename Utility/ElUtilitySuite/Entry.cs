﻿namespace ElUtilitySuite
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security.Permissions;

    using LeagueSharp;
    using LeagueSharp.Common;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    internal class Entry
    {
        #region Delegates

        /// <summary>
        /// Creates a new object of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        internal delegate T ObjectActivator<out T>(params object[] args);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        /// <value>
        /// The menu.
        /// </value>
        public static Menu Menu { get; set; }

        /// <summary>
        ///     Gets script version
        /// </summary>
        /// <value>
        ///     The script version
        /// </value>
        public static string ScriptVersion
        {
            get
            {
                return typeof(Entry).Assembly.GetName().Version.ToString();
            }
        }

        #endregion

        #region Public Methods and Operators

        public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();
            var param = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            for (var i = 0; i < paramsInfo.Length; i++)
            {
                var paramCastExp = Expression.Convert(
                    Expression.ArrayIndex(param, Expression.Constant(i)),
                    paramsInfo[i].ParameterType);

                argsExp[i] = paramCastExp;
            }

            return (ObjectActivator<T>)Expression.Lambda(typeof(ObjectActivator<T>), Expression.New(ctor, argsExp), param).Compile();
        }


        public static void OnLoad()
        {
            try
            {
                var plugins = Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsInterface).Select(x => GetActivator<IPlugin>(x.GetConstructors().First())(null));

                var menu = MainMenu.AddMenu("ElUtilitySuite", "ElUtilitySuite");

                foreach (var plugin in plugins)
                {
                    plugin.CreateMenu(menu);
                    plugin.Load();
                }

                menu.Add("usecombo", new KeyBind("Combo (Active)", false, KeyBind.BindTypes.HoldActive, 32));

                Menu = menu;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        public static bool getCombo()
        {
            return Menu["usecombo"].Cast<KeyBind>().CurrentValue;
        }

        #endregion
    }
}