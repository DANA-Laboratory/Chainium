namespace Chainium.Blockchain.Public.Faucet

open System.Text
open Hopac
open HttpFs.Client
open Newtonsoft.Json
open Chainium.Blockchain.Common
open Chainium.Blockchain.Public.Core.DomainTypes
open Chainium.Blockchain.Public.Core.Dtos

module NodeClient =

    let getAddressNonce nodeApiUrl (ChainiumAddress address) =
        sprintf "%s/address/%s" nodeApiUrl address
        |> Request.createUrl Get
        |> Request.responseAsString
        |> run
        |> JsonConvert.DeserializeObject<ChxBalanceStateDto>
        |> fun dto -> dto.Nonce |> Nonce

    let submitTx nodeApiUrl tx =
        sprintf "%s/tx" nodeApiUrl
        |> Request.createUrl Post
        |> Request.bodyStringEncoded tx (Encoding.UTF8)
        |> Request.responseAsString
        |> run
