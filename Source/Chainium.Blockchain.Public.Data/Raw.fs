namespace Chainium.Blockchain.Public.Data

open System.IO
open Newtonsoft.Json
open Chainium.Common
open Chainium.Blockchain.Common
open Chainium.Blockchain.Public.Core.DomainTypes
open Chainium.Blockchain.Public.Core.Dtos

module Raw =

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // General
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    type RawDataType =
        | Tx
        | TxResult
        | Block

    let private createFileName (dataType : RawDataType) (key : string) =
        sprintf "%s_%s" (unionCaseName dataType) key

    let private saveData (dataDir : string) (dataType : RawDataType) (key : string) data : Result<unit, AppErrors> =
        let dataTypeName = unionCaseName dataType
        try
            if not (Directory.Exists(dataDir)) then
                Directory.CreateDirectory(dataDir) |> ignore

            let fileName = createFileName dataType key
            let path = Path.Combine(dataDir, fileName)

            if File.Exists(path) then
                Result.appError (sprintf "%s %s already exists." dataTypeName key)
            else
                let json = data |> JsonConvert.SerializeObject
                File.WriteAllText(path, json)
                Ok ()
        with
        | ex ->
            Log.error ex.AllMessagesAndStackTraces
            Result.appError (sprintf "Saving %s %s failed" dataTypeName key)

    let private loadData<'T> (dataDir : string) (dataType : RawDataType) (key : string) : Result<'T, AppErrors> =
        let dataTypeName = unionCaseName dataType
        try
            let fileName = createFileName dataType key
            let path = Path.Combine(dataDir, fileName)

            if File.Exists(path) then
                File.ReadAllText path
                |> JsonConvert.DeserializeObject<'T>
                |> Ok
            else
                Result.appError (sprintf "%s %s not found." dataTypeName key)
        with
        | ex ->
            Log.error ex.AllMessagesAndStackTraces
            Result.appError (sprintf "Loading %s %s failed" dataTypeName key)

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Specific
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    let saveTx (dataDir : string) (TxHash txHash) (txEnvelopeDto : TxEnvelopeDto) : Result<unit, AppErrors> =
        saveData dataDir Tx txHash txEnvelopeDto

    let getTx (dataDir : string) (TxHash txHash) : Result<TxEnvelopeDto, AppErrors> =
        loadData<TxEnvelopeDto> dataDir Tx txHash

    let saveTxResult (dataDir : string) (TxHash txHash) (txResultDto : TxResultDto) : Result<unit, AppErrors> =
        saveData dataDir TxResult txHash txResultDto

    let getTxResult (dataDir : string) (TxHash txHash) : Result<TxResultDto, AppErrors> =
        loadData<TxResultDto> dataDir TxResult txHash

    let saveBlock (dataDir : string) (blockDto : BlockDto) : Result<unit, AppErrors> =
        saveData dataDir Block (string blockDto.Header.Number) blockDto

    let getBlock (dataDir : string) (BlockNumber blockNumber) : Result<BlockDto, AppErrors> =
        loadData<BlockDto> dataDir Block (string blockNumber)

    let blockExists (dataDir : string) (BlockNumber blockNumber) =
        blockNumber
        |> string
        |> createFileName Block
        |> fun fileName -> Path.Combine (dataDir, fileName)
        |> File.Exists
