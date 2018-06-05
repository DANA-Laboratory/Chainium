namespace Chainium.Blockchain.Public.Core

open System
open Chainium.Common
open Chainium.Blockchain.Public.Core
open Chainium.Blockchain.Public.Core.DomainTypes
open Chainium.Blockchain.Public.Core.Dtos
open Chainium.Blockchain.Public.Core.Events

module Mapping =

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Tx
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    let txStatusToCode txStatus : byte =
        match txStatus with
        | Pending -> 0uy
        | Processed processedStatus ->
            match processedStatus with
            | Success -> 1uy
            | Failure _ -> 2uy

    let txEnvelopeFromDto (dto : TxEnvelopeDto) : TxEnvelope =
        {
            RawTx = dto.Tx |> Convert.FromBase64String
            Signature =
                {
                    V = dto.V
                    R = dto.R
                    S = dto.S
                }
        }

    let txActionFromDto (action : TxActionDto) =
        match action.ActionData with
        | :? ChxTransferTxActionDto as a ->
            {
                ChxTransferTxAction.RecipientAddress = ChainiumAddress a.RecipientAddress
                Amount = ChxAmount a.Amount
            }
            |> ChxTransfer
        | :? AssetTransferTxActionDto as a ->
            {
                FromAccountHash = AccountHash a.FromAccount
                ToAccountHash = AccountHash a.ToAccount
                AssetCode = AssetCode a.AssetCode
                Amount = AssetAmount a.Amount
            }
            |> AssetTransfer
        | _ ->
            failwith "Invalid action type to map."

    let txFromDto sender hash (dto : TxDto) : Tx =
        {
            TxHash = hash
            Sender = sender
            Nonce = Nonce dto.Nonce
            Fee = ChxAmount dto.Fee
            Actions = dto.Actions |> List.map txActionFromDto
        }

    let txToTxInfoDto status (tx : Tx) : TxInfoDto =
        {
            TxHash = tx.TxHash |> (fun (TxHash h) -> h)
            SenderAddress = tx.Sender |> (fun (ChainiumAddress a) -> a)
            Nonce = tx.Nonce |> (fun (Nonce n) -> n)
            Fee = tx.Fee |> (fun (ChxAmount a) -> a)
            ActionCount = Convert.ToInt16 tx.Actions.Length
            Status = txStatusToCode status
        }

    let pendingTxInfoFromDto (dto : PendingTxInfoDto) : PendingTxInfo =
        {
            TxHash = TxHash dto.TxHash
            Sender = ChainiumAddress dto.SenderAddress
            Nonce = Nonce dto.Nonce
            Fee = ChxAmount dto.Fee
            ActionCount = dto.ActionCount
            AppearanceOrder = dto.AppearanceOrder
        }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Block
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    let blockHeaderFromDto (dto : BlockHeaderDto) : BlockHeader =
        {
            BlockHeader.Number = BlockNumber dto.Number
            Hash = BlockHash dto.Hash
            PreviousHash = BlockHash dto.PreviousHash
            Timestamp = Timestamp dto.Timestamp
            Validator = ChainiumAddress dto.Validator
            TxSetRoot = MerkleTreeRoot dto.TxSetRoot
            TxResultSetRoot = MerkleTreeRoot dto.TxResultSetRoot
            StateRoot = MerkleTreeRoot dto.StateRoot
        }

    let blockFromDto (dto : BlockDto) : Block =
        {
            Header = blockHeaderFromDto dto.Header
            TxSet = dto.TxSet |> List.map TxHash
        }

    let blockHeaderToDto (block : BlockHeader) : BlockHeaderDto =
        {
            BlockHeaderDto.Number = block.Number |> fun (BlockNumber n) -> n
            Hash = block.Hash |> fun (BlockHash h) -> h
            PreviousHash = block.PreviousHash |> fun (BlockHash h) -> h
            Timestamp = block.Timestamp |> fun (Timestamp t) -> t
            Validator = block.Validator |> fun (ChainiumAddress a) -> a
            TxSetRoot = block.TxSetRoot |> fun (MerkleTreeRoot r) -> r
            TxResultSetRoot = block.TxResultSetRoot |> fun (MerkleTreeRoot r) -> r
            StateRoot = block.StateRoot |> fun (MerkleTreeRoot r) -> r
        }

    let blockToDto (block : Block) : BlockDto =
        {
            Header = blockHeaderToDto block.Header
            TxSet = block.TxSet |> List.map (fun (TxHash h) -> h)
        }

    let blockInfoDtoFromBlockHeaderDto (blockHeaderDto : BlockHeaderDto) : BlockInfoDto =
        {
            BlockNumber = blockHeaderDto.Number
            BlockHash = blockHeaderDto.Hash
            BlockTimestamp = blockHeaderDto.Timestamp
        }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // State
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    let chxBalanceStateFromDto (dto : ChxBalanceStateDto) : ChxBalanceState =
        {
            Amount = ChxAmount dto.Amount
            Nonce = Nonce dto.Nonce
        }

    let chxBalanceStateToDto (state : ChxBalanceState) : ChxBalanceStateDto =
        {
            Amount = state.Amount |> (fun (ChxAmount a) -> a)
            Nonce = state.Nonce |> (fun (Nonce n) -> n)
        }

    let holdingStateFromDto (dto : HoldingStateDto) : HoldingState =
        {
            Amount = AssetAmount dto.Amount
        }

    let holdingStateToDto (state : HoldingState) : HoldingStateDto =
        {
            Amount = state.Amount |> (fun (AssetAmount a) -> a)
        }

    let outputToDto (output : ProcessingOutput) : ProcessingOutputDto =
        let txResults =
            output.TxResults
            |> Map.toList
            |> List.map (fun (TxHash h, s : TxProcessedStatus) -> h, s |> Processed |> txStatusToCode)
            |> Map.ofList

        let chxBalances =
            output.ChxBalances
            |> Map.toList
            |> List.map (fun (ChainiumAddress a, s : ChxBalanceState) -> a, chxBalanceStateToDto s)
            |> Map.ofList

        let holdings =
            output.Holdings
            |> Map.toList
            |> List.map (fun ((AccountHash ah, AssetCode ac), s : HoldingState) -> (ah, ac), holdingStateToDto s)
            |> Map.ofList

        {
            ProcessingOutputDto.TxResults = txResults
            ChxBalances = chxBalances
            Holdings = holdings
        }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Events
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    let txSubmittedEventToSubmitTxResponseDto (event : TxSubmittedEvent) =
        let (TxHash hash) = event.TxHash
        { SubmitTxResponseDto.TxHash = hash }
