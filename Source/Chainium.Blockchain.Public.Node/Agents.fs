namespace Chainium.Blockchain.Public.Node

open Chainium.Blockchain.Common
open Chainium.Blockchain.Public.Core.Events

module Agents =

    let private txPropagator = Agent.start <| fun (message : TxReceivedEventData) ->
        async {
            Composition.propagateTx message.TxHash
        }

    let private blockPropagator = Agent.start <| fun (message : BlockCreatedEventData) ->
        async {
            Composition.propagateBlock message.BlockNumber
        }

    let publishEvent event =
        Log.infof "EVENT: %A" event

        match event with
        | TxSubmitted e -> txPropagator.Post e
        | TxReceived e -> txPropagator.Post e
        | BlockCreated e -> blockPropagator.Post e
