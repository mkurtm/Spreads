﻿module Spreads.Period.PeriodTests
open System
open System.Diagnostics

open Spreads
open NUnit.Framework
open FsUnit

/// run f and measure ops per second
let perf (count:int64) (message:string) (f:unit -> unit) : unit = // int * int =
  GC.Collect(3, GCCollectionMode.Forced, true)
  let startMem = GC.GetTotalMemory(false)
  let sw = Stopwatch.StartNew()
  f()
  sw.Stop()
  let endtMem = GC.GetTotalMemory(true)
  let p = (1000L * count/sw.ElapsedMilliseconds)
  //int p, int((endtMem - startMem)/1024L)
  Console.WriteLine(message + ", #{0}, ops: {1}, mem/item: {2}", 
    count.ToString(), p.ToString(), ((endtMem - startMem)/count).ToString())




[<Test>]
let CreateManyTimePeriodFromDateTime() = 
    let count = 1000000L
    perf count "IntMap64<int64> insert" (fun _ ->
      for i in 0L..count do
        let tp = Spreads.TimePeriod(UnitPeriod.Millisecond, 1, 
                  DateTime.Today.AddMilliseconds(float i), TimeZoneInfo.Local)
        ()
    )

[<Test>]
let CreateManyTimePeriodFromDateTimeOffset() = 
    let count = 10000000L
    let initDTO = DateTimeOffset(2014,11,23,0,0,0,0, TimeSpan.Zero)
    perf count "TimePeriod from DTO" (fun _ ->
      let arr = Array.init (int count) (fun i -> Spreads.TimePeriod(UnitPeriod.Millisecond, 1, 
                      initDTO.AddMilliseconds(float i)))
//      for i in 0L..count do
//        let tp = Spreads.TimePeriod(UnitPeriod.Millisecond, 1, 
//                  DateTimeOffset(2014,11,23,0,0,0,0, TimeSpan.Zero).AddMilliseconds(float i))
//        ()
      arr.Length |> ignore
    )

[<Test>]
let CreateManyTimePeriodFromParts() = 
    let count = 1000000L
    perf count "IntMap64<int64> insert" (fun _ ->
      let arr = Array.init (int count) (fun i -> Spreads.TimePeriod(UnitPeriod.Millisecond, 1, 
                      2014, 11, 23, i / 3600000, i / 60000, i/1000, i))
//      for i in 0L..count do
//        let tp = Spreads.TimePeriod(UnitPeriod.Millisecond, 1, 
//                  DateTimeOffset(2014,11,23,0,0,0,0, TimeSpan.Zero).AddMilliseconds(float i))
//        ()
      arr.Length |> ignore
    )

