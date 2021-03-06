namespace Chainium.Blockchain.Public.Core.DomainTypes

open System

////////////////////////////////////////////////////////////////////////////////////////////////////
// Wallet
////////////////////////////////////////////////////////////////////////////////////////////////////

type PrivateKey = PrivateKey of string
type ChainiumAddress = ChainiumAddress of string

type WalletInfo = {
    PrivateKey : PrivateKey
    Address : ChainiumAddress
}

type Signature = {
    V : string
    R : string
    S : string
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// Accounts
////////////////////////////////////////////////////////////////////////////////////////////////////

type AccountHash = AccountHash of string
type AssetHash = AssetHash of string
type AssetCode = AssetCode of string

type Nonce = Nonce of int64
type ChxAmount = ChxAmount of decimal
type AssetAmount = AssetAmount of decimal

////////////////////////////////////////////////////////////////////////////////////////////////////
// Tx
////////////////////////////////////////////////////////////////////////////////////////////////////

type TxHash = TxHash of string

type TransferChxTxAction = {
    RecipientAddress : ChainiumAddress
    Amount : ChxAmount
}

type TransferAssetTxAction = {
    FromAccountHash : AccountHash
    ToAccountHash : AccountHash
    AssetHash : AssetHash
    Amount : AssetAmount
}

type CreateAssetEmissionTxAction = {
    EmissionAccountHash : AccountHash
    AssetHash : AssetHash
    Amount : AssetAmount
}

type SetAccountControllerTxAction = {
    AccountHash : AccountHash
    ControllerAddress : ChainiumAddress
}

type SetAssetControllerTxAction = {
    AssetHash : AssetHash
    ControllerAddress : ChainiumAddress
}

type SetAssetCodeTxAction = {
    AssetHash : AssetHash
    AssetCode : AssetCode
}

type SetValidatorNetworkAddressTxAction = {
    NetworkAddress : string
}

type TxAction =
    | TransferChx of TransferChxTxAction
    | TransferAsset of TransferAssetTxAction
    | CreateAssetEmission of CreateAssetEmissionTxAction
    | CreateAccount
    | CreateAsset
    | SetAccountController of SetAccountControllerTxAction
    | SetAssetController of SetAssetControllerTxAction
    | SetAssetCode of SetAssetCodeTxAction
    | SetValidatorNetworkAddress of SetValidatorNetworkAddressTxAction

type Tx = {
    TxHash : TxHash
    Sender : ChainiumAddress
    Nonce : Nonce
    Fee : ChxAmount
    Actions : TxAction list
}

type TxEnvelope = {
    RawTx : byte[]
    Signature : Signature
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// Block
////////////////////////////////////////////////////////////////////////////////////////////////////

type Timestamp = Timestamp of int64 // UNIX Timestamp
type BlockNumber = BlockNumber of int64
type BlockHash = BlockHash of string
type MerkleTreeRoot = MerkleTreeRoot of string

type BlockHeader = {
    Number : BlockNumber
    Hash : BlockHash
    PreviousHash : BlockHash
    Timestamp : Timestamp
    Validator : ChainiumAddress // Fee beneficiary
    TxSetRoot : MerkleTreeRoot
    TxResultSetRoot : MerkleTreeRoot
    StateRoot : MerkleTreeRoot
}

type Block = {
    Header : BlockHeader
    TxSet : TxHash list
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// Processing
////////////////////////////////////////////////////////////////////////////////////////////////////

type TxActionNumber = TxActionNumber of int16

type TxErrorCode =
    // CHANGING THESE NUMBERS WILL INVALIDATE TX RESULTS MERKLE ROOT IN EXISTING BLOCKS!!!

    // Address
    | NonceTooLow = 100s
    | InsufficientChxBalance = 110s

    // Holding
    | InsufficientAssetHoldingBalance = 210s

    // Account
    | AccountNotFound = 310s
    | AccountAlreadyExists = 320s
    | SenderIsNotSourceAccountController = 330s
    | SourceAccountNotFound = 340s
    | DestinationAccountNotFound = 350s

    // Asset
    | AssetNotFound = 410s
    | AssetAlreadyExists = 420s
    | SenderIsNotAssetController = 430s

type TxError =
    | TxError of TxErrorCode
    | TxActionError of TxActionNumber * TxErrorCode

type TxStatus =
    | Success
    | Failure of TxError

type TxResult = {
    Status : TxStatus
    BlockNumber : BlockNumber
}

type PendingTxInfo = {
    TxHash : TxHash
    Sender : ChainiumAddress
    Nonce : Nonce
    Fee : ChxAmount
    ActionCount : int16
    AppearanceOrder : int64
}

type ChxBalanceState = {
    Amount : ChxAmount
    Nonce : Nonce
}

type HoldingState = {
    Amount : AssetAmount
}

type AccountState = {
    ControllerAddress : ChainiumAddress
}

type AssetState = {
    AssetCode : AssetCode option
    ControllerAddress : ChainiumAddress
}

type ValidatorState = {
    NetworkAddress : string
}

type ProcessingOutput = {
    TxResults : Map<TxHash, TxResult>
    ChxBalances : Map<ChainiumAddress, ChxBalanceState>
    Holdings : Map<AccountHash * AssetHash, HoldingState>
    Accounts : Map<AccountHash, AccountState>
    Assets : Map<AssetHash, AssetState>
    Validators : Map<ChainiumAddress, ValidatorState>
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// Domain Type Logic
////////////////////////////////////////////////////////////////////////////////////////////////////

type BlockNumber with
    static member (+) (BlockNumber n1, BlockNumber n2) =
        BlockNumber (n1 + n2)
    static member (+) (BlockNumber n1, n2) =
        BlockNumber (n1 + n2)
    static member (-) (BlockNumber n1, BlockNumber n2) =
        BlockNumber (n1 - n2)
    static member (-) (BlockNumber n1, n2) =
        BlockNumber (n1 - n2)

type Nonce with
    static member (+) (Nonce n1, Nonce n2) =
        Nonce (n1 + n2)
    static member (+) (Nonce n1, n2) =
        Nonce (n1 + n2)
    static member (-) (Nonce n1, Nonce n2) =
        Nonce (n1 - n2)
    static member (-) (Nonce n1, n2) =
        Nonce (n1 - n2)

type ChxAmount with
    static member Zero =
        ChxAmount 0M
    static member (+) (ChxAmount a1, ChxAmount a2) =
        ChxAmount (Decimal.Round(a1 + a2, 18))
    static member (+) (ChxAmount a1, a2) =
        ChxAmount (Decimal.Round(a1 + a2, 18))
    static member (-) (ChxAmount a1, ChxAmount a2) =
        ChxAmount (Decimal.Round(a1 - a2, 18))
    static member (-) (ChxAmount a1, a2) =
        ChxAmount (Decimal.Round(a1 - a2, 18))
    static member (*) (ChxAmount a1, ChxAmount a2) =
        ChxAmount (Decimal.Round(a1 * a2, 18))
    static member (*) (ChxAmount a1, a2) =
        ChxAmount (Decimal.Round(a1 * a2, 18))
    static member (/) (ChxAmount a1, ChxAmount a2) =
        ChxAmount (Decimal.Round(a1 / a2, 18))
    static member (/) (ChxAmount a1, a2) =
        ChxAmount (Decimal.Round(a1 / a2, 18))

type AssetAmount with
    static member Zero =
        AssetAmount 0M
    static member (+) (AssetAmount a1, AssetAmount a2) =
        AssetAmount (Decimal.Round(a1 + a2, 18))
    static member (+) (AssetAmount a1, a2) =
        AssetAmount (Decimal.Round(a1 + a2, 18))
    static member (-) (AssetAmount a1, AssetAmount a2) =
        AssetAmount (Decimal.Round(a1 - a2, 18))
    static member (-) (AssetAmount a1, a2) =
        AssetAmount (Decimal.Round(a1 - a2, 18))
    static member (*) (AssetAmount a1, AssetAmount a2) =
        AssetAmount (Decimal.Round(a1 * a2, 18))
    static member (*) (AssetAmount a1, a2) =
        AssetAmount (Decimal.Round(a1 * a2, 18))
    static member (/) (AssetAmount a1, AssetAmount a2) =
        AssetAmount (Decimal.Round(a1 / a2, 18))
    static member (/) (AssetAmount a1, a2) =
        AssetAmount (Decimal.Round(a1 / a2, 18))

type Tx with
    member __.TotalFee = __.Fee * decimal __.Actions.Length

type PendingTxInfo with
    member __.TotalFee = __.Fee * decimal __.ActionCount

////////////////////////////////////////////////////////////////////////////////////////////////////
// Network
////////////////////////////////////////////////////////////////////////////////////////////////////

type NetworkAddress = NetworkAddress of string

type NetworkMessageId =
    | Tx of TxHash
    | Block of BlockNumber

type NetworkNodeConfig = {
    NetworkAddress : NetworkAddress
    BootstrapNodes : NetworkAddress list
}

type GossipMember = {
    NetworkAddress : NetworkAddress
    Heartbeat : int64
}

type GossipDiscoveryMessage = {
    ActiveMembers : GossipMember list
}

type GossipMessage = {
    MessageId : NetworkMessageId
    SenderAddress : NetworkAddress
    Data : obj
}

type MulticastMessage = {
    MessageId : NetworkMessageId
    Data : obj
}

type PeerMessage =
    | GossipDiscoveryMessage of GossipDiscoveryMessage
    | GossipMessage of GossipMessage
    | MulticastMessage of MulticastMessage
