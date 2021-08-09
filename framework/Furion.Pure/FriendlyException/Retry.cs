﻿// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             http://license.coscl.org.cn/MulanPSL2
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using System;
using System.Linq;
using System.Threading;

namespace Furion.FriendlyException
{
    /// <summary>
    /// 重试类
    /// </summary>
    public sealed class Retry
    {
        /// <summary>
        /// 重试有异常的方法，还可以指定特定异常
        /// </summary>
        /// <param name="action"></param>
        /// <param name="numRetries">重试次数</param>
        /// <param name="retryTimeout">重试间隔时间</param>
        /// <param name="exceptionTypes">异常类型,可多个</param>
        public static void Invoke(Action action, int numRetries, int retryTimeout, params Type[] exceptionTypes)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _ = Invoke(() =>
            {
                action();
                return 0;
            }, numRetries, retryTimeout, exceptionTypes);
        }

        /// <summary>
        /// 重试有异常的方法，还可以指定特定异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="numRetries">重试次数</param>
        /// <param name="retryTimeout">重试间隔时间</param>
        /// <param name="exceptionTypes">异常类型,可多个</param>
        public static T Invoke<T>(Func<T> action, int numRetries, int retryTimeout, params Type[] exceptionTypes)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            // 不断重试
            while (true)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    // 如果可重试次数小于或等于0，则终止重试
                    if (--numRetries <= 0) throw;

                    // 如果填写了 exceptionTypes 且异常类型不在 exceptionTypes 之内，则终止重试
                    if (exceptionTypes != null && exceptionTypes.Length > 0 && !exceptionTypes.Any(u => u.IsAssignableFrom(ex.GetType()))) throw;

                    // 如果可重试异常数大于 0，则间隔指定时间后继续执行
                    if (retryTimeout > 0) Thread.Sleep(retryTimeout);
                }
            }
        }
    }
}