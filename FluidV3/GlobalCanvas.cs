using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidV3
{
    static class GlobalValues //gg
    {
        /// <summary>
        ///  Начальная скорость
        /// </summary>
        public const double inVel = 0.000375;
        /// <summary>
        /// Квадрат ридиуса
        /// </summary>
        public const double rad2 = 0.0006250;
        /// <summary>
        /// Количество шагов
        /// </summary>
        public const int steps = 140001;
        /// <summary>
        /// Мастштаб сетки
        /// </summary>
        public const double scaleNet = 2.0;
        /// <summary>
        /// Шаг времени
        /// </summary>
        public const double dt = 1 / 960;
    }
}
