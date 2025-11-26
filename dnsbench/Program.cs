// //-------------------------------------------------------------------------------------------------
// // <copyright file="Program.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

using BenchmarkDotNet.Running;
using DnsBench;

// Run all benchmarks
BenchmarkSwitcher.FromAssembly(typeof(DnsProtocolBenchmarks).Assembly).Run(args);
