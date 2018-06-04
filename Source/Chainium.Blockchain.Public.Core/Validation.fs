﻿namespace Chainium.Blockchain.Public.Core

open System
open Chainium.Common
open Chainium.Blockchain.Common
open Chainium.Blockchain.Public.Core.DomainTypes
open Chainium.Blockchain.Public.Core.Dtos

module Validation =

    let validateTxEnvelope (txEnvelopeDto : TxEnvelopeDto) : Result<TxEnvelope, AppErrors> =
        [
            if txEnvelopeDto.Tx.IsNullOrWhiteSpace() then
                yield AppError "Tx is missing from the envelope."
            if txEnvelopeDto.V.IsNullOrWhiteSpace() then
                yield AppError "Signature component V is missing from the envelope."
            if txEnvelopeDto.R.IsNullOrWhiteSpace() then
                yield AppError "Signature component R is missing from the envelope."
            if txEnvelopeDto.S.IsNullOrWhiteSpace() then
                yield AppError "Signature component S is missing from the envelope."
        ]
        |> Errors.orElseWith (fun _ -> Mapping.txEnvelopeFromDto txEnvelopeDto)

    let verifyTxSignature verifySignature (txEnvelope : TxEnvelope) : Result<ChainiumAddress, AppErrors> =
        match verifySignature txEnvelope.Signature txEnvelope.RawTx with
        | Some chainiumAddress ->
            Ok chainiumAddress
        | None ->
            Error [AppError "Cannot verify signature"]

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Initial transaction validation
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    let private validateTxFields (t : TxDto) =
        [
            if t.Nonce <= 0L then
                yield AppError "Nonce must be positive."
            if t.Fee <= 0M then
                yield AppError "Fee must be positive."
            if t.Actions |> List.isEmpty then
                yield AppError "There are no actions provided for this transaction."
        ]

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Validation rules based on action type
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    let private validateChxTransfer chx =
        [
            if chx.RecipientAddress.IsNullOrWhiteSpace() then
                yield AppError "Recipient address is not valid."

            if chx.Amount <= 0M then
                yield AppError "Chx transfer amount must be larger than zero."
        ]

    let private validateEquityTransfer eq =
        [
            if eq.FromAccount.IsNullOrWhiteSpace() then
                yield AppError "FromAccount value is not valid."

            if eq.ToAccount.IsNullOrWhiteSpace() then
                yield AppError "ToAccount value is not valid."

            if eq.Equity.IsNullOrWhiteSpace() then
                yield AppError "Equity value is not valid."

            if eq.Amount <= 0M then
                yield AppError "Equity amount must be larger than zero"
        ]

    let private validateTxActions (actions : TxActionDto list) =
        let validateTxAction (action : TxActionDto) =
            match action.ActionData with
            | :? ChxTransferTxActionDto as chx ->
                validateChxTransfer chx
            | :? EquityTransferTxActionDto as eq ->
                validateEquityTransfer eq
            | _ ->
                let error = sprintf "Unknown action data type: %s" (action.ActionData.GetType()).FullName
                [AppError error]

        actions
        |> List.collect validateTxAction

    let validateTx sender hash (txDto : TxDto) : Result<Tx, AppErrors> =
        validateTxFields txDto @ validateTxActions txDto.Actions
        |> Errors.orElseWith (fun _ -> Mapping.txFromDto sender hash txDto)
