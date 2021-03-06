﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.


namespace Spreads

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open System.Diagnostics
open Spreads
open Microsoft.FSharp.Control

//
//
//
//// TODO rename back to MapCursor - this is an original cursor backed by some map, it does not represent series itself
//[<AbstractClassAttribute>]
//type BaseCursorOld<'K,'V>
//  (source:IReadOnlySeries<'K,'V>) as this =
//
//  // implement default MoveNextAsync logic using only MoveNext
//  let isUpdateable = match source with | :? IUpdateable -> true | _ -> false
//  let observerStarted = ref false
//  //let tcs = ref (TaskCompletionSource<bool>())
//  let mutable tcs = (Runtime.CompilerServices.AsyncTaskMethodBuilder<bool>.Create())
//  // TODO use CT while waiting on semaphore
//  let cancellationToken = ref CancellationToken.None
//  let sr = Object()
//  let semaphore = new SemaphoreSlim(0,Int32.MaxValue)
//  let taskCompleter = ref Unchecked.defaultof<Task<bool>>
//  let rec completeTcs() : Task<bool> = 
//    task {
//          let mutable cont = true
//          let waitTask = semaphore.WaitAsync(!cancellationToken).ContinueWith(fun _ -> true)
//          let! couldProceed = waitTask
//          //Debug.WriteLine("A")
//          if cont && couldProceed && !observerStarted && waitTask.IsCompleted then
//            lock(sr) (fun _ ->
//              // right now a client is waiting for a task to complete, there are no more elements in the map
//              if tcs <> Unchecked.defaultof<_> then
//                //Debug.WriteLine("B: !tcs <> null")
//                if this.MoveNext() then
//                  let tcs' = tcs
//                  tcs <- Unchecked.defaultof<_>
//                  //Console.WriteLine("C: Moved")
//                  //let couldSetResult = 
//                  (tcs').SetResult(true)
//        #if PRERELEASE
//                  //Trace.Assert(couldSetResult)
//        #endif
//                  ()
//                // check if the source became immutable
//                elif source.IsReadOnly then 
//                  //Debug.WriteLine("D")
//                  //let couldSetResult = 
//                  (tcs).SetResult(false)
//                  //Trace.Assert(couldSetResult)
//                  cont <- false
//              else
//                // do nothing, next MoveNext(ct) will try to call MoveNext() and it will return the correct result
//                //Console.WriteLine("Not moved")
//                ()
//            )
//            return! completeTcs()
//          else
//            //Debug.WriteLine("STOP")
//            return false // stop the loop
//    }
//
//  let updateHandler : OnUpdateHandler = 
//    OnUpdateHandler(fun _ ->
//        if semaphore.CurrentCount = 0 then semaphore.Release() |> ignore
//    )
//      
//  abstract Comparer: IComparer<'K> with get
//  override this.Comparer with get() = source.Comparer
//
//  abstract MoveAt: index:'K * direction:Lookup -> bool
//
//  abstract MoveFirst: unit -> bool
//
//  abstract MoveLast: unit -> bool
//
//  abstract member MoveNext : unit -> bool
//  
//  abstract MovePrevious: unit -> bool
//
//  abstract Current:KVP<'K,'V> with get
//  override this.Current with get(): KeyValuePair<'K, 'V> = KVP(this.CurrentKey, this.CurrentValue)
//
//  abstract CurrentKey:'K with get
//
//  abstract CurrentValue:'V with get
//
//  abstract Dispose: unit -> unit
//  override this.Dispose() =
//    lock(sr) (fun _ ->
//      if !observerStarted then
//        Trace.Assert(source :? IUpdateable)
//        (source :?> IUpdateable).remove_OnUpdate(updateHandler)
//    )
//
//  abstract member Reset : unit -> unit
//
//  abstract member MoveNext : CancellationToken -> Task<bool>
//  override this.MoveNext(ct) =
//      match this.MoveNext() with
//      | true -> trueTask
//      | false ->
//        match isUpdateable, source.IsReadOnly with
//        | true, false ->
//          let upd = source :?> IUpdateable
//          if not !observerStarted then 
//            upd.add_OnUpdate(updateHandler)
//            observerStarted := true
//            taskCompleter := completeTcs()
//
//          // TODO why spinning doesn't add performance?
//  //        let mutable moved = this.MoveNext()
//  ////        if not moved then
//  ////          let spinCountMax = 100
//  ////          let mutable spinCount = 0
//  ////          while spinCount < spinCountMax do
//  ////            if this.MoveNext() then 
//  ////              spinCount <- spinCountMax
//  ////              moved <- true
//  ////            else spinCount <- spinCount + 1
//  //        if moved && not ct.IsCancellationRequested then
//  //          //isWaitingForTcs := false
//  //          trueTask
//  //        else
//          cancellationToken := ct
//          // TODO use interlocked.exchange or whatever to not allocate new one every time, if we return trueTask below
//          // interlocked.exchange does not short-circuit and allocates value each time
//          //Interlocked.CompareExchange(tcs, TaskCompletionSource(), null) |> ignore
//        
////          if !tcs = null then
////            tcs := TaskCompletionSource()
////          tcs.Value.Task
//          // NB it looks like the fact that tcs is a struct is important
//          lock(sr) (fun _ ->
//              if tcs = Unchecked.defaultof<_>  then
//                tcs <- Runtime.CompilerServices.AsyncTaskMethodBuilder<bool>.Create() // new TaskCompletionSource<bool>() //
//              tcs.Task
//          )
//        
//        | _ -> falseTask // has no values and will never have because is not IUpdateable or IsMutable=false
//
//  abstract MoveNextBatchAsync: cancellationToken:CancellationToken  -> Task<bool>
//  abstract CurrentBatch: ISeries<'K,'V> with get
//  abstract Source : ISeries<'K,'V> with get
//  override this.Source with get() = source :> ISeries<'K,'V>
//  abstract Clone: unit -> ICursor<'K,'V>
//
//  // NB this is now not a part of interface (it was, could be back if needed)
//  abstract IsBatch: bool with get
//  abstract IsContinuous: bool with get
//
////  abstract Subscribe: observer:IObserver<KVP<'K,'V>> -> IDisposable
////  override this.Subscribe(observer : IObserver<KVP<'K,'V>>) : IDisposable =
////    match box observer with
////    | :? ISeriesSubscriber<'K, 'V> as seriesSubscriber -> 
////      let seriesSubscription : ISeriesSubscription<'K> = Unchecked.defaultof<_>
////      seriesSubscription :> IDisposable
////    | :? ISubscriber<KVP<'K,'V>> as subscriber -> 
////      let subscription : ISubscription = Unchecked.defaultof<_>
////      subscription :> IDisposable
////    | _ ->
////      // draft: move from current position on every source OnNext
////      let sourceObserver = 
////        { new IObserver<KVP<'K,'V>> with
////            member x.OnNext(kvp) = ()
////            member x.OnCompleted() = observer.OnCompleted()
////            member x.OnError(exn) = observer.OnError(exn)
////        }
////      let sourceSubscription = source.Subscribe(sourceObserver)
////      { new IDisposable with
////          member x.Dispose() = 
////            sourceSubscription.Dispose()
////      }
//
//
//  interface IDisposable with
//    member this.Dispose() = this.Dispose()
//
//  interface IEnumerator<KVP<'K,'V>> with
//    member this.Reset() = this.Reset()
//    member this.MoveNext():bool = this.MoveNext()
//    member this.Current with get(): KVP<'K, 'V> = this.Current
//    member this.Current with get(): obj = this.Current :> obj
//
//  interface IAsyncEnumerator<KVP<'K,'V>> with
//    member this.MoveNext(cancellationToken:CancellationToken): Task<bool> = this.MoveNext(cancellationToken) 
//
//  interface ICursor<'K,'V> with
//    member this.Comparer with get() = this.Comparer
//    member this.CurrentBatch: ISeries<'K,'V> = this.CurrentBatch
//    member this.MoveNextBatch(cancellationToken: CancellationToken): Task<bool> = this.MoveNextBatchAsync(cancellationToken)
//    //member this.IsBatch with get() = this.IsBatch
//    member this.MoveAt(index:'K, lookup:Lookup) = this.MoveAt(index, lookup)
//    member this.MoveFirst():bool = this.MoveFirst()
//    member this.MoveLast():bool =  this.MoveLast()
//    member this.MovePrevious():bool = this.MovePrevious()
//    member this.CurrentKey with get():'K = this.CurrentKey
//    member this.CurrentValue with get():'V = this.CurrentValue
//    member this.Source with get() = this.Source
//    member this.Clone() = this.Clone()
//    member this.IsContinuous with get() = this.IsContinuous
//    member this.TryGetValue(key, [<Out>]value: byref<'V>) : bool = source.TryGetValue(key, &value)
//

// TODO rename back to MapCursor - this is an original cursor backed by some map, it does not represent series itself
[<AbstractClassAttribute>]
type BaseCursor<'K,'V>(source:IReadOnlySeries<'K,'V>) =
      
  abstract Comparer: IComparer<'K> with get
  override this.Comparer with get() = source.Comparer

  abstract MoveAt: index:'K * direction:Lookup -> bool

  abstract MoveFirst: unit -> bool

  abstract MoveLast: unit -> bool

  abstract member MoveNext : unit -> bool
  
  abstract MovePrevious: unit -> bool

  abstract Current:KVP<'K,'V> with get
  override this.Current with get(): KeyValuePair<'K, 'V> = KVP(this.CurrentKey, this.CurrentValue)

  abstract CurrentKey:'K with get

  abstract CurrentValue:'V with get

  abstract Dispose: unit -> unit
  override this.Dispose() = ()

  abstract member Reset : unit -> unit

  abstract member MoveNext : CancellationToken -> Task<bool>
  override this.MoveNext(ct) = falseTask
    
  abstract MoveNextBatchAsync: cancellationToken:CancellationToken  -> Task<bool>
  abstract CurrentBatch: IReadOnlySeries<'K,'V> with get
  abstract Source : IReadOnlySeries<'K,'V> with get
  override this.Source with get() = source :> IReadOnlySeries<'K,'V>
  abstract Clone: unit -> ICursor<'K,'V>
  abstract IsContinuous: bool with get


  interface IDisposable with
    member this.Dispose() = this.Dispose()

  interface IEnumerator<KVP<'K,'V>> with
    member this.Reset() = this.Reset()
    member this.MoveNext():bool = this.MoveNext()
    member this.Current with get(): KVP<'K, 'V> = this.Current
    member this.Current with get(): obj = this.Current :> obj

  interface IAsyncEnumerator<KVP<'K,'V>> with
    member this.MoveNext(cancellationToken:CancellationToken): Task<bool> = this.MoveNext(cancellationToken) 

  interface ICursor<'K,'V> with
    member this.Comparer with get() = this.Comparer
    member this.CurrentBatch = this.CurrentBatch
    member this.MoveNextBatch(cancellationToken: CancellationToken): Task<bool> = this.MoveNextBatchAsync(cancellationToken)
    member this.MoveAt(index:'K, lookup:Lookup) = this.MoveAt(index, lookup)
    member this.MoveFirst():bool = this.MoveFirst()
    member this.MoveLast():bool =  this.MoveLast()
    member this.MovePrevious():bool = this.MovePrevious()
    member this.CurrentKey with get():'K = this.CurrentKey
    member this.CurrentValue with get():'V = this.CurrentValue
    member this.Source with get() = this.Source
    member this.Clone() = this.Clone()
    member this.IsContinuous with get() = this.IsContinuous
    member this.TryGetValue(key, [<Out>]value: byref<'V>) : bool = source.TryGetValue(key, &value)


/// Uses IReadOnlySeries's TryFind method, doesn't know anything about underlying sequence
type MapCursor<'K,'V>(map:IReadOnlySeries<'K,'V>) =
  inherit BaseCursor<'K,'V>(map)
  [<DefaultValue>] 
  val mutable private currentPosition : bool * KeyValuePair<'K,'V>

  let mutable isReset = true

  override this.MoveAt(index:'K, lookup:Lookup) = 
    isReset <- false
    this.currentPosition <- map.TryFind(index, lookup)
    fst this.currentPosition

  override this.MoveFirst():bool = 
    try
      this.MoveAt(map.First.Key, Lookup.EQ)
    with
      | :? InvalidOperationException -> false

  override this.MoveLast():bool =
    try
      this.MoveAt(map.Last.Key, Lookup.EQ)
    with
      | :? InvalidOperationException -> false

  override this.MoveNext():bool = 
    if isReset then this.MoveFirst()
    else
      this.currentPosition <- map.TryFind((snd this.currentPosition).Key, Lookup.GT)
      fst this.currentPosition
  
  override this.MovePrevious():bool = 
    if isReset then this.MoveLast()
    else
      this.currentPosition <- map.TryFind((snd this.currentPosition).Key, Lookup.LT)
      fst this.currentPosition

  override this.Current 
    with get(): KeyValuePair<'K, 'V> = 
      snd this.currentPosition

  override this.CurrentKey with get():'K = this.Current.Key

  override this.CurrentValue with get():'V = this.Current.Value

  override this.Reset() = isReset <- true

  override this.CurrentBatch = raise (NotSupportedException("IReadOnlySeries do not support batches, override the method in a map implementation"))
  override this.MoveNextBatchAsync(cancellationToken: CancellationToken): Task<bool> = raise (NotSupportedException("IReadOnlySeries do not support batches, override the method in a map implementation"))

  override this.Source with get() = map

  override this.Clone() =
    let c = new MapCursor<'K,'V>(map)
    c.currentPosition <- this.currentPosition
    c :> ICursor<'K,'V>

  override this.IsContinuous with get() = false


